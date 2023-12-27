import {Button, Modal} from "react-bootstrap";
import {Input} from "reactstrap";
import {useState} from "react";
import {apiEndpoint} from "../../api";
import {launchError, launchSuccess} from "../../components/layout/Layout";

const RequestModal = ({show, handleClose, workId}) => {
    const [description, setDescription] = useState('');

    const handleSend = () => {
        apiEndpoint('contractor/send-request').post({workId, description})
            .then(() => {
                setDescription('');
                handleClose();
                launchSuccess({data: 'Request sent successfully'});
            }).catch(err => launchError(err));
    }

    const handleCancel = () => {
        setDescription('');
        handleClose();
    }

    return (
        <Modal show={show} onHide={handleCancel}>
            <Modal.Header closeButton>
                <Modal.Title>
                    Add comment to request
                </Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Input type={"textarea"} placeholder={"Enter description"} className={"mb-2"} value={description}
                       onChange={(e) => setDescription(e.target.value)}/>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="primary" onClick={handleSend}>
                    Send
                </Button>
            </Modal.Footer>
        </Modal>
    )
}

export default RequestModal;