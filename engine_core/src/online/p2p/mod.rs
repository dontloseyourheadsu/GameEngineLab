pub mod crypto;
pub mod node;
pub mod packet;

pub use node::P2PNode;
pub use packet::Packet;

#[cfg(test)]
mod tests {
    use super::*;
    use node::ConnectionState;
    use std::net::{IpAddr, Ipv4Addr, SocketAddr};
    use std::thread;
    use std::time::Duration;

    #[test]
    fn test_p2p_connection_and_messaging() {
        // Bind with port 0 to let OS assign an available port
        let mut node_a = P2PNode::new(0).expect("Failed to bind node A");
        let mut node_b = P2PNode::new(0).expect("Failed to bind node B");

        // Get the assigned addresses (assuming localhost)
        // Note: local_addr() returns the bound address. If we bound to 0.0.0.0, it returns 0.0.0.0.
        // We need to use 127.0.0.1 for the connect call.
        let port_a = node_a.socket.local_addr().unwrap().port();
        let port_b = node_b.socket.local_addr().unwrap().port();

        let _addr_a = SocketAddr::new(IpAddr::V4(Ipv4Addr::new(127, 0, 0, 1)), port_a);
        let addr_b = SocketAddr::new(IpAddr::V4(Ipv4Addr::new(127, 0, 0, 1)), port_b);

        println!("Node A on {}, Node B on {}", port_a, port_b);

        // A connects to B
        node_a.connect(addr_b);

        // Simulation loop
        let msg = b"Hello Secure World";
        let mut msg_sent = false;
        let mut msg_received = false;

        for i in 0..100 {
            node_a.update();
            node_b.update();

            // When B receives A's Hello (which happens implicitly by A sending it),
            // B will set A as peer.
            // Note: In my implementation, B accepts peer if peer_addr is none.

            // Wait for connection
            if node_a.state == ConnectionState::Connected
                && node_b.state == ConnectionState::Connected
            {
                if !msg_sent {
                    println!("Both connected! Sending message...");
                    node_a.send_message(msg.to_vec());
                    msg_sent = true;
                }
            }

            if let Some(received) = node_b.receive_message() {
                assert_eq!(received, msg);
                println!("Message received successfully!");
                msg_received = true;
                break;
            }

            thread::sleep(Duration::from_millis(50));
        }

        assert!(msg_received, "Message was not received");
    }
}
