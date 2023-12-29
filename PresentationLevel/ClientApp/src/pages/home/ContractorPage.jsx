import {useEffect, useState} from "react";
import {useNavigate} from "react-router-dom";
import {apiEndpoint} from "../../api";
import {Button, Card} from "react-bootstrap";
import {Input} from "reactstrap";
import RequestModal from "../requests/RequestModal";

const ContractorPage = () => {
    const [projects, setProjects] = useState([]);
    const [search, setSearch] = useState('');
    const [open, setOpen] = useState(false);
    const [workId, setWorkId] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        apiEndpoint('contractor/home').fetch()
            .then(res => {
                setProjects(res.data.works);
            }).catch(() => navigate('/login'));
    }, [navigate]);

    const handleSearch = (e) => {
        e.preventDefault();

        apiEndpoint('contractor/search').post(search)
            .then(res => setProjects(res.data))
            .catch(() => navigate('/login'));
    }

    return (
        <>
            <div className={"d-flex flex-column align-items-center mt-2"}>
                <h1 className={'mt-2'}>Available projects</h1>
                <form className={"d-flex flex-row gap-4 w-75 mt-2 mb-2"} onSubmit={handleSearch}>
                    <Input type={"text"} placeholder={"Search by name"} value={search}
                           onChange={(e) => setSearch(e.target.value)}/>
                    <Button variant={"outline-primary"} className={"ml-2"} type={'submit'}>Search</Button>
                </form>
                {
                    projects.length === 0 ? <h3>No projects found</h3> :
                        projects.map((project) =>
                            <Card className={'w-75 mt-3'}>
                                <Card.Body>
                                    <Card.Title>{project.name}</Card.Title>
                                    <Card.Text>{project.description}</Card.Text>
                                    <Card.Text>Deadline: {project['deadline'] ? new Date(project['deadline']).toLocaleDateString() : 'Not specified'}</Card.Text>
                                    <Card.Text>Price: {project['price'] != null ? `$${project['price'].toFixed(2)}` : 'Not specified'}</Card.Text>
                                    <Button variant="outline-success" className={"mt-2 w-100"} onClick={() => {
                                        setWorkId(project.id);
                                        setOpen(true);
                                    }}>
                                        Send offer
                                    </Button>
                                </Card.Body>
                            </Card>
                        )
                }
                <RequestModal workId={workId} show={open} handleClose={() => setOpen(false)}/>
            </div>
        </>
    )
}

export default ContractorPage;