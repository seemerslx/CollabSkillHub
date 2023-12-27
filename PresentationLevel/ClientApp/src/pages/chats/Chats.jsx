import React, {useEffect, useState} from 'react';
import {Container, Row, Col, ListGroup, Form} from 'react-bootstrap';
import {InputGroup, FormControl, Button} from 'react-bootstrap';
import {FontAwesomeIcon} from '@fortawesome/react-fontawesome';
import {faPaperPlane} from '@fortawesome/free-solid-svg-icons';
import {useNavigate, useParams} from "react-router-dom";
import signalRService from "../../api/signalR";
import {launchError} from "../../components/layout/Layout";
import {apiEndpoint} from "../../api";

const Chats = () => {
    const [activeChat, setActiveChat] = useState(null);
    const [chats, setChats] = useState([]);
    const [messages, setMessages] = useState([]);
    const [show, setShow] = useState(false);
    const id = useParams().id;
    const navigate = useNavigate();

    const handleChatSelection = (chat) => {
        navigate(`/chats/${chat.id}`);
    };

    useEffect(() => {
        if (!id)
            return;

        apiEndpoint('chat').fetchById(id)
            .then((response) => {
                setActiveChat(response.data);
                setMessages(response.data.messages);
            })
            .catch((error) => launchError(error));
    }, [id]);

    useEffect(() => {
        apiEndpoint('chat').fetch()
            .then((response) => {
                setChats(response.data['chats']);
                setShow(response.data.show);
            })
            .catch((error) => launchError(error));
    }, []);

    useEffect(() => {
        signalRService.onReceiveMessage((message) => {
            setMessages([...messages, message]);

            setTimeout(() => {
                const scroll = document.getElementById('scroll');
                scroll.scrollTop = scroll.scrollHeight;
            }, 100);
        });
    }, [messages]);

    const handleSendClick = (event) => {
        event.preventDefault();

        const message = event.target[0].value;
        event.target[0].value = '';

        if (!message)
            return;

        signalRService.sendMessage(message, +id)
            .catch((error) => launchError(error));
    };

    const handleClose = () => {
        apiEndpoint('customer/close-chat/' + id).post()
            .then(() => navigate(0))
            .catch((error) => launchError(error));
    }

    return (
        <Container fluid className="h-100">
            <Row className="h-100">
                <Col xs={3} className="bg-light p-3">
                    <ListGroup>
                        {
                            chats.map((chat) => (
                                <ListGroup.Item
                                    key={chat.id}
                                    action
                                    active={activeChat && activeChat.id === chat.id}
                                    onClick={() => handleChatSelection(chat)}
                                >
                                    {chat.name} {chat['isArchived'] && '(Archived)'}
                                </ListGroup.Item>
                            ))
                        }
                    </ListGroup>
                </Col>

                <Col xs={9} className="d-flex flex-column h-100 p-3">
                    {activeChat ? (
                        <>
                            <div className="d-flex justify-content-between align-items-center mb-3">
                                <h4>
                                    {activeChat.name} {activeChat['isArchived'] && '(Archived)'}
                                </h4>
                                {
                                    show && !activeChat['isArchived'] &&
                                    <Button variant="success" onClick={handleClose}>Close Conversation</Button>
                                }
                            </div>

                            <div style={{height: '65vh', overflowY: 'auto'}} id={'scroll'}>
                                {
                                    messages.map((message, index) => (
                                        <div
                                            key={message.id}
                                            className={message.sender === activeChat.name ? 'text-right' : 'text-left'}
                                        >
                                            {messages[index - 1]?.sender !== message.sender && <div className={'mt-3'}>
                                                <strong>{message.sender}</strong>
                                            </div>}
                                            <div className={'ms-4'}>
                                                {message.text}
                                            </div>
                                        </div>
                                    ))
                                }
                            </div>

                            {
                                !activeChat['isArchived'] && <Form className="mt-3" onSubmit={handleSendClick}>
                                    <InputGroup>
                                        <FormControl
                                            placeholder="Type your message..."
                                            aria-label="Type your message"
                                            aria-describedby="basic-addon2"
                                        />
                                        <Button variant="primary" type={'submit'}>
                                            <FontAwesomeIcon icon={faPaperPlane}/>
                                        </Button>
                                    </InputGroup>
                                </Form>
                            }
                        </>
                    ) : (
                        <div className="d-flex align-items-center justify-content-center"
                             style={{height: '78vh'}}>
                            <p>Select a chat to start conversation</p>
                        </div>
                    )}
                </Col>
            </Row>
        </Container>
    );
};

export default Chats;
