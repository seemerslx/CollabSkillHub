import React, {useEffect, useState} from 'react';
import {Container, Row, Col, ListGroup, Form, ButtonGroup} from 'react-bootstrap';
import {InputGroup, FormControl, Button} from 'react-bootstrap';
import {FontAwesomeIcon} from '@fortawesome/react-fontawesome';
import {faPaperPlane, faCheck, faCreditCard} from '@fortawesome/free-solid-svg-icons';
import {useNavigate, useParams} from "react-router-dom";
import signalRService from "../../api/signalR";
import {launchError} from "../../components/layout/Layout";
import {apiEndpoint} from "../../api";
import CommentPopup from "../forms/CommentPopup";
import PaymentModal from "../../components/PaymentModel";

const Chats = () => {
    const [activeChat, setActiveChat] = useState(null);
    const [chats, setChats] = useState([]);
    const [messages, setMessages] = useState([]);
    const [show, setShow] = useState(false);
    const [userRole, setUserRole] = useState(''); // 'customer' or 'contractor'
    const [workState, setWorkState] = useState('');
    const [canPay, setCanPay] = useState(false);
    const [canMarkReady, setCanMarkReady] = useState(false);
    const [showPaymentModal, setShowPaymentModal] = useState(false);
    const id = useParams().id;
    const navigate = useNavigate();
    const [open, setOpen] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    const handleChatSelection = (chat) => {
        navigate(`/chats/${chat.id}`);
    };

    const refreshChatData = () => {
        if (!id) return;

        apiEndpoint('chat').fetchById(id)
            .then((response) => {
                setActiveChat(response.data.chat);
                setMessages(response.data.chat.messages);
                
                // Set user role from response
                setUserRole(response.data.userRole || '');

                // Set work state if work exists
                if (response.data.chat.work) {
                    setWorkState(response.data.chat.work.state);
                    
                    // Set flags based on work state and user role
                    const isContractor = response.data.userRole === 'contractor';
                    const isCustomer = response.data.userRole === 'customer';
                    
                    // Contractor can mark as ready if work is in progress
                    setCanMarkReady(isContractor && response.data.chat.work.state === 'Inprogress');
                    
                    // Customer can pay if work is ready for review
                    setCanPay(isCustomer && response.data.chat.work.state === 'ReadyForReviewAndPay');
                }
            })
            .catch((error) => launchError(error));
    };

    useEffect(() => {
        refreshChatData();
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
            if (message['chatId'] !== +id)
                return;

            setMessages([...messages, message]);

            setTimeout(() => {
                const scroll = document.getElementById('scroll');
                if (scroll) {
                    scroll.scrollTop = scroll.scrollHeight;
                }
            }, 100);
        });
    }, [id, messages]);

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
        setOpen(true);
    };

    const handleMarkAsReady = () => {
        setIsLoading(true);
        apiEndpoint(`contractor/mark-as-ready/${id}`).post()
            .then(() => {
                // Refresh chat data to update UI
                refreshChatData();
                // Optional: Show a success message
                alert('Work marked as ready for review!');
            })
            .catch((error) => {
                launchError(error);
            })
            .finally(() => {
                setIsLoading(false);
            });
    };

    const handlePayClick = () => {
        setShowPaymentModal(true);
    };

    const handlePaymentSuccess = () => {
        // Refresh chat data after successful payment
        refreshChatData();
        setShowPaymentModal(false);
    };

    const getWorkStatusBadge = () => {
        if (!activeChat || !activeChat.work) return null;
        
        let badgeClass = '';
        let text = activeChat.work.state;
        
        switch (activeChat.work.state) {
            case 'InProgress':
                badgeClass = 'bg-info';
                text = 'In Progress';
                break;
            case 'ReadyForReviewAndPay':
                badgeClass = 'bg-warning';
                text = 'Ready for Review';
                break;
            case 'Completed':
                badgeClass = 'bg-success';
                text = 'Completed';
                break;
            default:
                badgeClass = 'bg-secondary';
        }
        
        return (
            <span className={`badge ${badgeClass} ms-2`}>{text}</span>
        );
    };

    return (
        <Container fluid className="h-100">
            <Row className="h-100">
                <Col xs={3} className="bg-light p-3">
                    <ListGroup>
                        {
                            chats.length === 0 ? <h3>No chats</h3> :
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
                                    {getWorkStatusBadge()}
                                </h4>
                                <ButtonGroup>
                                    {/* Contractor: Mark as Ready button */}
                                    {canMarkReady && (
                                        <Button 
                                            variant="primary" 
                                            onClick={handleMarkAsReady}
                                            disabled={isLoading}
                                            className="me-2"
                                        >
                                            <FontAwesomeIcon icon={faCheck} className="me-1" />
                                            Mark as Ready
                                        </Button>
                                    )}
                                    
                                    {/* Customer: Pay button */}
                                    {canPay && (
                                        <Button 
                                            variant="primary" 
                                            onClick={handlePayClick}
                                            disabled={isLoading}
                                            className="me-2"
                                        >
                                            <FontAwesomeIcon icon={faCreditCard} className="me-1" />
                                            Pay
                                        </Button>
                                    )}
                                    
                                    {/* Close Conversation button */}
                                    {show && !activeChat['isArchived'] && (
                                        <Button 
                                            variant="danger" 
                                            onClick={handleClose}
                                        >
                                            Close Conversation
                                        </Button>
                                    )}
                                </ButtonGroup>
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
                    <CommentPopup show={open} handleClose={() => setOpen(false)} id={id} />
                    
                    {/* Payment Modal */}
                    {showPaymentModal && activeChat && activeChat.work && (
                        <PaymentModal
                            workId={activeChat.work.id}
                            onClose={() => setShowPaymentModal(false)}
                            onSuccess={handlePaymentSuccess}
                        />
                    )}
                </Col>
            </Row>
        </Container>
    );
};

export default Chats;