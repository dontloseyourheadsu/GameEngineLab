pub mod cert;
pub mod node;

pub use node::P2PNode;

#[cfg(test)]
mod tests {
    use super::node::{P2PNode, read_stream, send_msg};
    use std::net::{IpAddr, Ipv4Addr, SocketAddr};

    #[tokio::test]
    async fn test_p2p_connection_and_messaging_quic() -> anyhow::Result<()> {
        // Create two nodes
        let node_a = P2PNode::new(0)?;
        let node_b = P2PNode::new(0)?;

        let port_a = node_a.local_addr()?.port();
        let port_b = node_b.local_addr()?.port();

        println!("Node A on {}, Node B on {}", port_a, port_b);

        let addr_b = SocketAddr::new(IpAddr::V4(Ipv4Addr::new(127, 0, 0, 1)), port_b);

        // We need to accept on Node B.
        let endpoint_b = node_b.endpoint.clone();

        let accept_task = tokio::spawn(async move {
            if let Some(incoming) = endpoint_b.accept().await {
                println!("Node B: incoming connection...");
                let connection = incoming.await.expect("Failed to accept connection");
                println!("Node B: connected from {}", connection.remote_address());

                // Wait for a message (stream)
                if let Ok(recv) = connection.accept_uni().await {
                    let msg = read_stream(recv).await.expect("Failed to read stream");
                    println!(
                        "Node B: received message: {:?}",
                        String::from_utf8_lossy(&msg)
                    );
                    return Some(msg);
                }
            }
            None
        });

        // Node A connects
        let conn_a = node_a.connect(addr_b, "localhost").await?;
        println!("Node A: connected to B");

        let msg = b"Hello QUIC World";
        send_msg(&conn_a, msg).await?;

        let received_msg = accept_task.await?;
        assert_eq!(received_msg, Some(msg.to_vec()));

        Ok(())
    }
}
