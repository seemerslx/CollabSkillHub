import React, {useEffect, useState} from 'react';
import {Container, Row, Col, Card, ListGroup, Form, Button} from 'react-bootstrap';
import {useParams} from "react-router-dom";
import {apiEndpoint} from "../../api";
import {launchError, launchSuccess} from "../../components/layout/Layout";

const ReviewPage = () => {
    const [info, setInfo] = useState(null);
    const id = useParams().id;

    useEffect(() => {
        if (id)
            apiEndpoint('customer/get-reviews').fetchById(id)
                .then(res => setInfo(res.data))
                .catch(err => launchError(err));
        else
            apiEndpoint('contractor/me').fetch()
                .then(res => setInfo(res.data))
                .catch(err => launchError(err));
    }, [id]);

    const getInitials = (name) => {
        const nameArray = name.split(' ');
        return nameArray
            .map((word) => word.charAt(0).toUpperCase())
            .join('');
    };

    const handleChange = (e) => {
        const {name, value} = e.target;
        setInfo({...info, [name]: value});
    }

    const update = () => {
        apiEndpoint('contractor/update-description').post(info.description)
            .then(res => launchSuccess(res))
            .catch(err => launchError(err));
    }

    if (!info)
        return null;

    return (
        <Container>
            <Row className="mt-4 mb-4">
                <Col md={4}>
                    <Card
                        className="text-center"
                        style={{
                            backgroundColor: '#007BFF',
                            color: 'white',
                            padding: '20px',
                        }}
                    >
                        <Card.Body>
                            <Card.Text style={{fontSize: '48px'}}>
                                {getInitials(info.fullName)}
                            </Card.Text>
                        </Card.Body>
                    </Card>
                </Col>
                <Col md={8}>
                    <h2>{info.fullName}</h2>
                </Col>
            </Row>

            {
                id ?
                    <>
                        <h3>Description</h3>
                        <p>{info.description}</p>
                    </>
                    :
                    <>
                        <Form.Group controlId="formDescription" className={'mb-4'}>
                            <Form.Label>Description (max 1000 symbols)</Form.Label>
                            <Form.Control
                                as="textarea"
                                rows={3}
                                placeholder="Enter work description"
                                name="description"
                                value={info.description || ''}
                                onChange={handleChange}
                                required
                                maxLength={1000}
                            />
                        </Form.Group>
                        <Button onClick={update}>Update</Button>
                    </>
            }

            <Row className="mt-4">
                <Col>
                    <h3>Reviews</h3>
                    {info['reviews'].length === 0 ? (
                        <p>No reviews yet.</p>
                    ) : (
                        <ListGroup>
                            {info['reviews'].map((review) => (
                                <ListGroup.Item key={review.id}>
                                    <strong>Rating: {review.stars}</strong>
                                    <p>{review.comment}</p>
                                    <p>Left by: {review['customer']?.fullName}</p>
                                </ListGroup.Item>
                            ))}
                        </ListGroup>
                    )}
                </Col>
            </Row>
        </Container>
    );
}

export default ReviewPage;