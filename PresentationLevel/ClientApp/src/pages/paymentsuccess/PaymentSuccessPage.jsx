import React, { useEffect, useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { apiEndpoint } from '../../api';
import "../Payment.css";

const PaymentSuccessPage = () => {
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [paymentDetails, setPaymentDetails] = useState(null);
    const navigate = useNavigate();
    const location = useLocation();

    useEffect(() => {
        const capturePayment = async () => {
            try {
                // Parse URL parameters
                const searchParams = new URLSearchParams(location.search);
                const paymentId = searchParams.get('paymentId');
                const token = searchParams.get('token'); // PayPal order ID

                if (!token || !paymentId) {
                    setError('Missing payment information');
                    setLoading(false);
                    return;
                }

                console.log("request mark as paid", paymentId);
                const markedPaidResponse = await apiEndpoint(`customer/mark-as-payed/${paymentId}`).post();
                console.log("response mark as paid", markedPaidResponse);

                console.log("request capture", token);
                const response = await apiEndpoint(`payment/paypal/capture/${token}`).post();
                console.log("Response capture", response);
                setPaymentDetails(response.data);
                setLoading(false);

                // Clean up the URL
                window.history.replaceState({}, document.title, '/payment/success');

            } catch (err) {
                setError(err.response?.data?.message || 'Failed to process payment');
                setLoading(false);
            }
        };

        capturePayment();
    }, [location.search]);

    const handleReturnToWorks = () => {
        navigate('/customer/works');
    };

    if (loading) {
        return (
            <div className="payment-success-container">
                <div className="payment-success-card loading">
                    <h2>Processing Payment</h2>
                    <p>Please wait while we confirm your payment...</p>
                    <div className="loading-spinner"></div>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="payment-success-container">
                <div className="payment-success-card error">
                    <h2>Payment Error</h2>
                    <p className="error-message">{error}</p>
                    <button className="return-button" onClick={handleReturnToWorks}>
                        Return to Works
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="payment-success-container">
            <div className="payment-success-card">
                <div className="success-icon">
                    <svg viewBox="0 0 24 24" width="64" height="64">
                        <circle cx="12" cy="12" r="11" fill="#4CAF50" />
                        <path d="M9 16.2L5.5 12.7 4.3 14 9 18.7 20 7.7 18.8 6.5 9 16.2z" fill="white" />
                    </svg>
                </div>
                <h2>Payment Successful!</h2>
                <p>Thank you for your payment. Your transaction has been completed successfully.</p>

                <div className="payment-details">
                    <p><strong>Transaction ID:</strong> {paymentDetails?.transactionId}</p>
                    <p><strong>Payment ID:</strong> {paymentDetails?.paymentId}</p>
                </div>

                <button className="return-button" onClick={handleReturnToWorks}>
                    Return to Works
                </button>
            </div>
        </div>
    );
};

export default PaymentSuccessPage;