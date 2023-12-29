import React, {useState} from 'react';
import {Modal, Button, Form} from 'react-bootstrap';
import {apiEndpoint} from "../../api";
import {launchError} from "../../components/layout/Layout";
import {useNavigate} from "react-router-dom";

const CommentPopup = ({show, handleClose, id}) => {
    const [comment, setComment] = useState('');
    const [rating, setRating] = useState(1);
    const navigate = useNavigate();

    const handleRatingChange = (event) => {
        setRating(+event.target.value);
    };

    const handleCommentChange = (event) => {
        setComment(event.target.value);
    };

    const handleSubmit = () => {
        const data = {
            comment,
            stars: rating
        };

        apiEndpoint('customer/close-chat/' + id).post(data)
            .then(() => {
                handleClose();
                navigate(0);
            })
            .catch((error) => launchError(error));
    };

    return (
        <Modal show={show} onHide={handleClose}>
            <Modal.Header closeButton>
                <Modal.Title>Enter Comment and Rating</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Form>
                    <Form.Group controlId="commentForm">
                        <Form.Label>Comment</Form.Label>
                        <Form.Control
                            as="textarea"
                            rows={3}
                            value={comment}
                            onChange={handleCommentChange}
                        />
                    </Form.Group>
                    <Form.Group controlId="ratingForm">
                        <Form.Label>Rating</Form.Label>
                        <Form.Control
                            as="select"
                            value={rating}
                            onChange={handleRatingChange}
                        >
                            {[1, 2, 3, 4, 5].map((value) => (
                                <option key={value} value={value}>
                                    {value}
                                </option>
                            ))}
                        </Form.Control>
                    </Form.Group>
                </Form>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={handleClose}>
                    Close
                </Button>
                <Button variant="primary" onClick={handleSubmit}>
                    Submit
                </Button>
            </Modal.Footer>
        </Modal>
    );
};

export default CommentPopup;