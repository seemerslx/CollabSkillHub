import React, { useState, useEffect } from 'react';
import { Modal, Button, Spinner, Alert, Card } from 'react-bootstrap';
import { apiEndpoint } from '../api';

const PaymentModal = ({ workId, onClose, onSuccess }) => {
    const [payment, setPayment] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [paymentProcessing, setPaymentProcessing] = useState(false);

    const paymentApi = apiEndpoint(`payment/create/${workId}`);
    const paypalCreateApi = apiEndpoint(`payment/paypal/create/${payment?.id}`);
    const paypalCaptureApi = apiEndpoint('payment/paypal/capture');

    useEffect(() => {
        const createPayment = async () => {
            try {
                setLoading(true);
                const response = await paymentApi.post();
                setPayment(response.data);
                setLoading(false);
            } catch (err) {
                setError(err.response?.data?.message || 'Failed to create payment');
                setLoading(false);
            }
        };

        createPayment();
    }, [workId]);

    const handlePayWithPayPal = async () => {
        try {
            setPaymentProcessing(true);
            const response = await paypalCreateApi.post();

            // Redirect to PayPal approval URL
            window.location.href = response.data.approvalUrl;

        } catch (err) {
            setError(err.response?.data?.message || 'Failed to create PayPal order');
            setPaymentProcessing(false);
        }
    };

    // This would be called after returning from PayPal
    const handlePayPalReturn = async (orderId) => {
        try {
            setPaymentProcessing(true);
            await paypalCaptureApi.post({ orderId });
            setPaymentProcessing(false);
            onSuccess();
            return true;
        } catch (err) {
            setError(err.response?.data?.message || 'Failed to capture payment');
            setPaymentProcessing(false);
            return false;
        }
    };

    // Check if the URL contains PayPal return parameters
    useEffect(() => {
        const url = new URL(window.location.href);
        const orderId = url.searchParams.get('token'); // PayPal returns the order ID as 'token'

        if (orderId) {
            handlePayPalReturn(orderId);

            // Clean up the URL
            window.history.replaceState({}, document.title, window.location.pathname);
        }
    }, []);

    return (
        <Modal show={true} onHide={onClose} centered backdrop="static" keyboard={!paymentProcessing}>
            <Modal.Header closeButton={!paymentProcessing}>
                <Modal.Title>
                    {loading ? 'Processing' : error ? 'Error' : 'Make Payment'}
                </Modal.Title>
            </Modal.Header>

            <Modal.Body>
                {loading ? (
                    <div className="text-center p-4">
                        <Spinner animation="border" role="status" className="mb-3" />
                        <p>Preparing payment...</p>
                    </div>
                ) : error ? (
                    <Alert variant="danger">{error}</Alert>
                ) : (
                    <>
                        <Card className="mb-4">
                            <Card.Header as="h5">Payment Details</Card.Header>
                            <Card.Body>
                                <p><strong>Work:</strong> {payment?.work?.name}</p>
                                <p><strong>Amount:</strong> ${payment?.amount.toFixed(2)}</p>
                                <p><strong>Contractor:</strong> {payment?.contractor?.userName}</p>
                            </Card.Body>
                        </Card>

                        <h5>Select Payment Method</h5>

                        {paymentProcessing ? (
                            <div className="d-flex align-items-center mt-3">
                                <Spinner animation="border" size="sm" className="me-2" />
                                <span>Processing payment...</span>
                            </div>
                        ) : (
                            <div className="d-grid gap-2 mt-3">
                                <Button
                                    variant="primary"
                                    size="lg"
                                    onClick={handlePayWithPayPal}
                                    disabled={paymentProcessing}
                                    className="d-flex align-items-center justify-content-center"
                                >
                                    <img
                                        src="https://www.paypalobjects.com/webstatic/en_US/i/buttons/PP_logo_h_100x26.png"
                                        alt="PayPal"
                                        height="25"
                                        className="me-2"
                                    />
                                    Pay with PayPal
                                </Button>
                            </div>
                        )}
                    </>
                )}
            </Modal.Body>

            <Modal.Footer>
                <Button
                    variant="secondary"
                    onClick={onClose}
                    disabled={paymentProcessing}
                >
                    Cancel
                </Button>
            </Modal.Footer>
        </Modal>
    );
};

export default PaymentModal;