import React, {useState} from 'react';
import {Form, Button, InputGroup} from 'react-bootstrap';
import {Link, useNavigate} from 'react-router-dom';
import {apiEndpoint} from "../../api";
import {launchError} from "../../components/layout/Layout";

const LoginForm = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const navigate = useNavigate();

    const handleUsernameChange = (e) => {
        setUsername(e.target.value);
    };

    const handlePasswordChange = (e) => {
        setPassword(e.target.value);
    };

    const handleLogin = (e) => {
        e.preventDefault();

        const data = {
            username,
            password
        };

        apiEndpoint('auth/login').post(data)
            .then(res => {
                localStorage.setItem('bearer_token', res.data.token);
                navigate('/');
            }).catch(err => launchError(err));
    };

    return (
        <div className={'d-flex flex-column justify-content-center align-items-center mt-5 gap-4'}>
            <h2>Login</h2>
            <Form onSubmit={handleLogin} className={'d-flex flex-column gap-4 w-75 mb-2'}>
                <Form.Group controlId="formBasicUsername">
                    <Form.Label>Username</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>@</InputGroup.Text>
                        <Form.Control type="username" placeholder="Enter username" value={username}
                                      onChange={handleUsernameChange}/>
                    </InputGroup>
                </Form.Group>
                <Form.Group controlId="formBasicPassword">
                    <Form.Label>Password</Form.Label>
                    <Form.Control type="password" placeholder="Password" value={password}
                                  onChange={handlePasswordChange}/>
                </Form.Group>

                <Button variant="primary" type="submit">
                    Login
                </Button>
            </Form>

            <p>
                Don't have an account? <Link to="/register">Register here</Link>
            </p>
        </div>
    );
};

export default LoginForm;
