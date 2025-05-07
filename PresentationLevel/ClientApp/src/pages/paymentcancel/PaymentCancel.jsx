import React from 'react';
import { useNavigate } from 'react-router-dom';
import "../Payment.css";

const PaymentCancelPage = () => {
  const navigate = useNavigate();

  const handleReturnToWorks = () => {
    navigate('/customer/works');
  };

  return (
    <div className="payment-cancel-container">
      <div className="payment-cancel-card">
        <div className="cancel-icon">
          <svg viewBox="0 0 24 24" width="64" height="64">
            <circle cx="12" cy="12" r="11" fill="#F44336" />
            <path d="M13.41,12l4.3-4.29a1,1,0,1,0-1.42-1.42L12,10.59,7.71,6.29A1,1,0,0,0,6.29,7.71L10.59,12l-4.3,4.29a1,1,0,0,0,0,1.42,1,1,0,0,0,1.42,0L12,13.41l4.29,4.3a1,1,0,0,0,1.42,0,1,1,0,0,0,0-1.42Z" fill="white" />
          </svg>
        </div>
        <h2>Payment Cancelled</h2>
        <p>Your payment process was cancelled. No payment has been processed.</p>
        
        <button className="return-button" onClick={handleReturnToWorks}>
          Return to Works
        </button>
      </div>
    </div>
  );
};

export default PaymentCancelPage;