import React, {useEffect, useState} from 'react';
import {Form, Button, InputGroup} from 'react-bootstrap';
import {apiEndpoint} from "../../api";
import {launchError} from "../../components/layout/Layout";
import {useNavigate, useParams} from "react-router-dom";

const AddWorkForm = () => {
    const navigate = useNavigate();
    const {id} = useParams();

    const [formData, setFormData] = useState({
        name: '',
        description: '',
        deadline: null,
        price: null,
        state: 'Active',
    });

    useEffect(() => {
        if (id)
            apiEndpoint('customer/work').fetchById(id)
                .then(res => setFormData(res.data))
                .catch(err => launchError(err));
    }, [id]);

    const handleChange = (e) => {
        const {name, value} = e.target;
        setFormData({
            ...formData,
            [name]: value,
        });
    };

    const handleSubmit = (e) => {
        e.preventDefault();

        if (id)
            apiEndpoint('customer/' + id).put(formData)
                .then(res => {
                    navigate('/dashboard');
                    setFormData({
                        name: '',
                        description: '',
                        deadline: null,
                        price: null,
                        state: 'Active',
                    });
                })
                .catch(err => launchError(err));
        else
            apiEndpoint('customer').post(formData)
                .then(res => {
                    navigate('/dashboard');
                    setFormData({
                        name: '',
                        description: '',
                        deadline: null,
                        price: null,
                        state: 'Active',
                    });
                })
                .catch(err => launchError(err));
    };

    return (
        <div className={"d-flex flex-column align-items-center mt-2"}>
            <h1>Add Work</h1>
            <Form onSubmit={handleSubmit} className={'d-flex flex-column gap-4 w-75 mb-2'}>
                <Form.Group controlId="formName">
                    <Form.Label>Name</Form.Label>
                    <Form.Control
                        type="text"
                        placeholder="Enter work name"
                        name="name"
                        value={formData.name}
                        onChange={handleChange}
                        required
                        maxLength={256}
                    />
                </Form.Group>

                <Form.Group controlId="formDescription">
                    <Form.Label>Description (max 500 symbols)</Form.Label>
                    <Form.Control
                        as="textarea"
                        rows={3}
                        placeholder="Enter work description"
                        name="description"
                        value={formData.description}
                        onChange={handleChange}
                        required
                        maxLength={500}
                    />
                </Form.Group>

                <Form.Group controlId="formDeadline">
                    <Form.Label>Deadline</Form.Label>
                    <Form.Control
                        type="date"
                        name="deadline"
                        value={formData.deadline}
                        onChange={handleChange}
                        min={new Date().toISOString().split('T')[0]}
                    />
                </Form.Group>

                <Form.Group controlId="formPrice">
                    <Form.Label>Price</Form.Label>
                    <InputGroup>
                        <InputGroup.Text>$</InputGroup.Text>
                        <Form.Control
                            type="number"
                            placeholder="Enter price"
                            name="price"
                            value={formData.price || ''}
                            onChange={handleChange}
                        />
                    </InputGroup>
                </Form.Group>

                <Form.Group controlId="formState">
                    <Form.Label>State</Form.Label>
                    <Form.Control
                        as="select"
                        name="state"
                        value={formData.state}
                    >
                        <option value={formData.state}>{formData.state}</option>
                    </Form.Control>
                </Form.Group>

                <Button variant="primary" type="submit">
                    Save
                </Button>
            </Form>
        </div>
    );
};

export default AddWorkForm;
