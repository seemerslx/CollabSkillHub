import React from 'react';
import {Container, Row, Col} from 'react-bootstrap';

const AppFooter = () => {
    return (
        <footer className="mt-5">
            <Container>
                <Row>
                    <Col className="text-center">
                        <p>&copy; 2025 Collab Skill Hub. All rights reserved.</p>
                    </Col>
                </Row>
            </Container>
        </footer>
    );
};

export default AppFooter;
