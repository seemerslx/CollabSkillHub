import React, {useState} from 'react';
import {Link, useNavigate, useParams} from 'react-router-dom';
import {Form, Button, InputGroup} from 'react-bootstrap';
import {apiEndpoint} from "../../api";
import {launchError} from "../../components/layout/Layout";

const RegistrationForm = () => {
    const {accountType} = useParams();

    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [email, setEmail] = useState('');
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');

    const navigate = useNavigate();

    const handleFirstNameChange = (e) => {
        setFirstName(e.target.value);
    };

    const handleLastNameChange = (e) => {
        setLastName(e.target.value);
    }

    const handleEmailChange = (e) => {
        setEmail(e.target.value);
    };

    const handlePasswordChange = (e) => {
        setPassword(e.target.value);
    };

    const handleUsernameChange = (e) => {
        setUsername(e.target.value);
    }

    const handleRegister = (e) => {
        e.preventDefault();

        const data = {
            firstName,
            lastName,
            email,
            username,
            password,
            type: accountType
        };

        apiEndpoint('auth/register').post(data)
            .then(res => {
                localStorage.setItem('bearer_token', res.data.token);
                navigate('/');
            })
            .catch(err => launchError(err));
    };

    if (accountType !== 'customer' && accountType !== 'contractor') {
        return <AccountTypeSelection/>
    }

    return (
        <div className={'d-flex flex-column justify-content-center align-items-center mt-5 gap-4'}>
            <h2>Register {accountType}</h2>
            <Form onSubmit={handleRegister} className={'d-flex flex-column gap-4 w-75 mb-2'}>
                <div className={'d-flex gap-4 w-100 justify-content-between'}>
                    <Form.Group controlId="formBasicName" className={'w-100'}>
                        <Form.Label>First name</Form.Label>
                        <Form.Control type="text" placeholder="Enter your first name" value={firstName}
                                      onChange={handleFirstNameChange}/>
                    </Form.Group>

                    <Form.Group controlId="formBasicLastName" className={'w-100'}>
                        <Form.Label>Last name</Form.Label>
                        <Form.Control type="text" placeholder="Enter your last name" value={lastName}
                                      onChange={handleLastNameChange}/>
                    </Form.Group>
                </div>

                <Form.Group controlId="formBasicEmail">
                    <Form.Label>Email address</Form.Label>
                    <Form.Control type="email" placeholder="Enter email" value={email} onChange={handleEmailChange}/>
                </Form.Group>

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
                    Register
                </Button>
            </Form>

            <p>
                Already have an account? <Link to="/login">Login here</Link>
            </p>
        </div>
    );
};

const AccountTypeSelection = () => {
    return (
        <div className={'d-flex flex-column justify-content-center align-items-center mt-5 gap-4'}>
            <h2>Choose Account Type</h2>
            <p>What type of account would you like to create?</p>
            <div className={'d-flex gap-4'}>
                <Link to="/register/customer">
                    <Button type="button" variant="outline-primary">
                        Customer Account
                    </Button>
                </Link>
                <Link to="/register/contractor">
                    <Button type="button" variant="outline-success">
                        Contractor Account
                    </Button>
                </Link>
            </div>
        </div>
    );
};

export default RegistrationForm;
