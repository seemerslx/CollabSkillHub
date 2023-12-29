import {useEffect, useState} from "react";
import {Nav} from "react-bootstrap";
import {apiEndpoint} from "../../api";
import {Link, useNavigate} from "react-router-dom";
import {launchError} from "../../components/layout/Layout";

const RequestPage = () => {
    const [requests, setRequests] = useState([]);
    const [activeKey, setActiveKey] = useState("Pending");
    const navigate = useNavigate();

    const handleSelect = (selectedKey) => setActiveKey(selectedKey);

    useEffect(() => {
        apiEndpoint('customer/requests').fetch()
            .then(res => setRequests(res.data))
            .catch(() => navigate('/login'));
    }, [navigate]);

    const handleButton = (id, accept) => {
        apiEndpoint(`customer/requests/${id}/${accept}`).post()
            .then(res => setRequests(res.data))
            .catch((err) => launchError(err));
    }

    return (
        <div className={'d-flex flex-column gap-4 mt-4 align-items-center'}>
            <h1 className={'d-flex justify-content-center'}>Requests</h1>
            <Nav variant="tabs" activeKey={activeKey} onSelect={handleSelect} className={'w-75'}>
                <Nav.Item>
                    <Nav.Link eventKey="Pending">Pending</Nav.Link>
                </Nav.Item>
                <Nav.Item>
                    <Nav.Link eventKey="Accepted">Accepted</Nav.Link>
                </Nav.Item>
                <Nav.Item>
                    <Nav.Link eventKey="Rejected">Rejected</Nav.Link>
                </Nav.Item>
            </Nav>
            <div className="mt-3 w-75">
                {
                    requests.filter(request => request.state === activeKey).map(request => (
                        <div key={request.id} className="card mb-3">
                            <div className="card-body">
                                <h5 className="card-title">{request['work']?.name}</h5>
                                <h6 className="card-subtitle mb-2 text-muted">
                                    Request from: <Link
                                    to={'/reviews/' + request['contractor']?.id}>{request['contractor']?.fullName}</Link>
                                </h6>
                                <h6 className="card-subtitle mb-2 text-muted">
                                    For the work: {request['work']?.name}
                                </h6>
                                <p className="card-text">{request.description}</p>
                                <p className="card-text">
                                    <small className="text-muted">{new Date(request.createdAt).toLocaleString()}</small>
                                </p>
                                {
                                    request.state === 'Pending' && (
                                        <div className="d-flex justify-content-end">
                                            <button className={'btn btn-success me-2'}
                                                    onClick={() => handleButton(request.id, true)}>
                                                Accept
                                            </button>
                                            <button className={'btn btn-danger'}
                                                    onClick={() => handleButton(request.id, false)}>
                                                Reject
                                            </button>
                                        </div>
                                    )
                                }
                            </div>
                        </div>
                    ))
                }
                {
                    requests.filter(request => request.state === activeKey).length === 0 && (
                        <div className="alert alert-info">No requests</div>
                    )
                }
            </div>
        </div>
    );
}

export default RequestPage;