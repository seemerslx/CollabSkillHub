import {Button, Card} from "react-bootstrap";
import {useEffect, useState} from "react";
import {apiEndpoint} from "../../api";
import {useNavigate} from "react-router-dom";

const ClientPage = () => {
    const [projects, setProjects] = useState([]);
    const navigate = useNavigate();

    useEffect(() => {
        apiEndpoint('customer').fetch()
            .then(res => {
                setProjects(res.data['groupWorks']);
            }).catch(() => navigate('/login'));
    }, [navigate]);

    return (
        <>
            <div className={"d-flex flex-column align-items-center mt-2"}>
                <h1>My projects</h1>
                <Button variant="outline-primary" className={"mt-2 w-75 mb-2"}
                        onClick={() => navigate('/dashboard/form')}>
                    Add project
                </Button>
                {
                    projects.map((project) =>
                        <>
                            <h3 className={'mt-3'}>{project.state}</h3>
                            {
                                project.works.map((work) =>
                                    <Card className={'w-75 mt-3'}>
                                        <Card.Body>
                                            <Card.Title>{work.name}</Card.Title>
                                            <Card.Text>{work.description}</Card.Text>
                                            <Card.Text>Deadline: {work['deadline'] ? new Date(work['deadline']).toLocaleDateString() : 'Not specified'}</Card.Text>
                                            <Card.Text>Price: {work['price'] != null ? `$${work['price'].toFixed(2)}` : 'Not specified'}</Card.Text>
                                            <Card.Text>State: {work.state}</Card.Text>
                                        </Card.Body>
                                    </Card>
                                )
                            }
                        </>
                    )
                }
            </div>
        </>
    )
}

export default ClientPage;