import React, {useEffect, useState} from 'react';
import {Navbar, Nav} from 'react-bootstrap';
import {apiEndpoint} from "../../api";
import {useNavigate} from "react-router-dom";

const AppNavbar = () => {
    const [nav, setNav] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        apiEndpoint('auth/verify').fetch()
            .then((res) => {
                if (res.data === 'Contractor')
                    setNav('contractor')
                else if (res.data === 'Admin')
                    setNav('')
                else if (res.data === 'Customer')
                    setNav('customer')
                else
                    setNav('')
            })
            .catch(() => setNav(''))
    }, [navigate]);

    return (
        <Navbar bg="dark" variant="dark" expand="lg" className={'ps-3'}>
            <Navbar.Brand href="/">Collab Skill Hub</Navbar.Brand>
            <Navbar.Toggle aria-controls="basic-navbar-nav"/>
            <Navbar.Collapse id="basic-navbar-nav">
                <Nav className="ml-auto">
                    {nav === 'customer' && <Nav.Link href="/dashboard">Home</Nav.Link>}
                    {nav === 'customer' && <Nav.Link href="/requests">Requests</Nav.Link>}
                    {nav === 'contractor' && <Nav.Link href="/works">Home</Nav.Link>}
                    {nav !== '' && <Nav.Link href="/chats">Chats</Nav.Link>}
                    {nav !== '' &&
                        <Nav.Link href="/login"
                                  onClick={() => localStorage.removeItem('bearer_token')}>Logout</Nav.Link>}
                    {nav === '' && <Nav.Link href="/login">Login</Nav.Link>}
                </Nav>
            </Navbar.Collapse>
        </Navbar>
    );
};

export default AppNavbar;
