import React, { useState } from 'react';
import PaymentModal from './PaymentModal';
import { apiEndpoint } from '../utils/apiEndpoint';
import "../Payment.css";

const PaymentButton = ({ work }) => {
  const [showPaymentModal, setShowPaymentModal] = useState(false);
  const workUpdateApi = apiEndpoint('work/mark-as-paid');

  const handleInitiatePayment = () => {
    setShowPaymentModal(true);
  };

  const handlePaymentSuccess = async () => {
    try {
      // Update the work status to paid if needed
      await workUpdateApi.post({ workId: work.id });
      
      // Trigger any parent component updates (such as refreshing the works list)
      if (typeof window.updateWorksList === 'function') {
        window.updateWorksList();
      }
      
      // Close the modal
      setShowPaymentModal(false);
    } catch (error) {
      console.error('Error updating work status:', error);
    }
  };

  const handleClose = () => {
    setShowPaymentModal(false);
  };

  // Only show the payment button for completed works that haven't been paid
  if (work.state !== 'Completed' || work.isPaid) {
    return null;
  }

  return (
    <>
      <button 
        className="pay-button" 
        onClick={handleInitiatePayment}
      >
        Pay Contractor
      </button>

      {showPaymentModal && (
        <PaymentModal 
          workId={work.id}
          onClose={handleClose}
          onSuccess={handlePaymentSuccess}
        />
      )}
    </>
  );
};

export default PaymentButton;