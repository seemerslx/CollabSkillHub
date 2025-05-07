import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiEndpoint } from '../../api'; // Fixed import path
import './ContractorProfilePage.css';

const ContractorProfilePage = () => {
    const [profile, setProfile] = useState(null);
    const [paymentInfo, setPaymentInfo] = useState({
        payPalEmail: '',
        defaultPaymentMethod: 'PayPal'
    });
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [successMessage, setSuccessMessage] = useState('');
    const navigate = useNavigate();

    const contractorApi = apiEndpoint('contractor/profile');
    const paymentInfoApi = apiEndpoint('contractor/payment-info');

    useEffect(() => {
        const fetchProfileData = async () => {
            try {
                setLoading(true);

                // Fetch contractor profile
                const profileResponse = await contractorApi.fetch();
                setProfile(profileResponse.data);

                // Fetch payment info
                const paymentInfoResponse = await paymentInfoApi.fetch();
                if (paymentInfoResponse.data) {
                    setPaymentInfo({
                        payPalEmail: paymentInfoResponse.data.payPalEmail || '',
                        defaultPaymentMethod: paymentInfoResponse.data.defaultPaymentMethod || 'PayPal'
                    });
                }

                setLoading(false);
            } catch (err) {
                setError(err.response?.data?.message || 'Failed to load profile data');
                setLoading(false);
            }
        };

        fetchProfileData();
    }, []);

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setPaymentInfo(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            setLoading(true);

            await paymentInfoApi.post(paymentInfo);

            setSuccessMessage('Payment information updated successfully!');
            setTimeout(() => setSuccessMessage(''), 3000);

            setLoading(false);
        } catch (err) {
            setError(err.response?.data?.message || 'Failed to update payment information');
            setLoading(false);
        }
    };

    if (loading && !profile) {
        return (
            <div className="profile-container loading">
                <div className="loading-spinner"></div>
                <p>Loading profile...</p>
            </div>
        );
    }

    if (error) {
        return (
            <div className="profile-container error">
                <h2>Error</h2>
                <p className="error-message">{error}</p>
                <button onClick={() => navigate('/dashboard')}>Back to Dashboard</button>
            </div>
        );
    }

    return (
        <div className="profile-container">
            <div className="profile-header">
                <h1>Contractor Profile</h1>
                <p>Set up your profile and payment information to receive payments</p>
            </div>

            {successMessage && (
                <div className="success-message">
                    {successMessage}
                </div>
            )}

            <div className="profile-content">
                <div className="profile-section">
                    <h2>Personal Information</h2>
                    {profile && (
                        <div className="profile-info">
                            <p><strong>Name:</strong> {profile.firstName} {profile.lastName}</p>
                            <p><strong>Email:</strong> {profile.email}</p>
                            <p><strong>Username:</strong> {profile.userName}</p>
                            <p><strong>Description:</strong> {profile.description || 'Not specified'}</p>
                        </div>
                    )}
                    <button className="edit-button" onClick={() => navigate('/contractor/edit-profile')}>
                        Edit Profile
                    </button>
                </div>

                <div className="payment-info-section">
                    <h2>Payment Information</h2>
                    <p className="payment-info-desc">
                        To receive payments from clients, please enter your PayPal email address.
                        This email will be used to direct payments to your PayPal account.
                    </p>

                    <form onSubmit={handleSubmit} className="payment-info-form">
                        <div className="form-group">
                            <label htmlFor="payPalEmail">PayPal Email Address:</label>
                            <input
                                type="email"
                                id="payPalEmail"
                                name="payPalEmail"
                                value={paymentInfo.payPalEmail}
                                onChange={handleInputChange}
                                placeholder="your-email@example.com"
                                required
                            />
                            <small>This must be a valid PayPal account email address</small>
                        </div>

                        <div className="form-group">
                            <label htmlFor="defaultPaymentMethod">Default Payment Method:</label>
                            <select
                                id="defaultPaymentMethod"
                                name="defaultPaymentMethod"
                                value={paymentInfo.defaultPaymentMethod}
                                onChange={handleInputChange}
                            >
                                <option value="PayPal">PayPal</option>
                                {/* Add more payment methods here as needed */}
                            </select>
                        </div>

                        <button type="submit" className="save-button" disabled={loading}>
                            {loading ? 'Saving...' : 'Save Payment Information'}
                        </button>
                    </form>

                    <div className="payment-status">
                        <h3>Payment Setup Status</h3>
                        <div className={`status-indicator ${paymentInfo.payPalEmail ? 'complete' : 'incomplete'}`}>
                            {paymentInfo.payPalEmail
                                ? '✓ You can receive payments'
                                : '✗ You need to set up payment info'}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ContractorProfilePage;