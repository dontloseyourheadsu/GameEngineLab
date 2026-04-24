use super::cert::{SkipServerVerification, configure_client, configure_server};
use anyhow::Result;
use base64::{Engine as _, engine::general_purpose::STANDARD as BASE64};
use quinn::Endpoint;
use std::net::{IpAddr, Ipv4Addr, SocketAddr};
use std::sync::Arc;
use std::time::Duration;

/// A P2P Node wrapper around Quinn Endpoint
pub struct P2PNode {
    pub endpoint: Endpoint,
}

impl P2PNode {
    /// Create a new P2P Node listening on the specified port.
    /// If port is 0, OS assigns a random port.
    pub fn new(port: u16) -> Result<Self> {
        let _ = rustls::crypto::ring::default_provider().install_default();

        let (server_config, _server_cert) = configure_server()?;
        let mut client_config = configure_client(Arc::new(SkipServerVerification))?;

        let mut endpoint = Endpoint::server(
            server_config,
            SocketAddr::new(IpAddr::V4(Ipv4Addr::new(0, 0, 0, 0)), port),
        )?;

        let mut transport_config = quinn::TransportConfig::default();
        transport_config.keep_alive_interval(Some(Duration::from_secs(5)));
        client_config.transport_config(Arc::new(transport_config));

        endpoint.set_default_client_config(client_config);

        Ok(Self { endpoint })
    }

    pub fn local_addr(&self) -> Result<SocketAddr> {
        Ok(self.endpoint.local_addr()?)
    }

    /// Connect to a peer
    pub async fn connect(&self, addr: SocketAddr, server_name: &str) -> Result<quinn::Connection> {
        let connection = self.endpoint.connect(addr, server_name)?.await?;
        Ok(connection)
    }

    /// Generates a base64 connection link containing IP and port.
    /// If external_ip is provided, it uses that; otherwise it uses the bounds IP (which is 0.0.0.0, not useful for sharing).
    /// You should pass the machine's public or LAN IP here.
    pub fn generate_invite_link(&self, external_ip: IpAddr) -> Result<String> {
        let port = self.local_addr()?.port();
        let s = format!("{}:{}", external_ip, port);
        Ok(BASE64.encode(s))
    }

    /// Connects to a peer using the invite link.
    pub async fn connect_via_link(&self, link: &str) -> Result<quinn::Connection> {
        let bytes = BASE64.decode(link)?;
        let s = String::from_utf8(bytes)?;
        let addr: SocketAddr = s.parse()?;
        // server_name "localhost" is a placeholder since we skip verification
        self.connect(addr, "localhost").await
    }
}

// Helper to read all bytes from a stream
pub async fn read_stream(mut recv: quinn::RecvStream) -> Result<Vec<u8>> {
    let data = recv.read_to_end(1024 * 64).await?;
    Ok(data)
}

// Helper to send data as a uni-directional stream
pub async fn send_msg(conn: &quinn::Connection, data: &[u8]) -> Result<()> {
    let mut send = conn.open_uni().await?;
    send.write_all(data).await?;
    send.finish()?;
    Ok(())
}
