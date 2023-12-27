import {useNavigate} from "react-router-dom";
import {useEffect} from "react";
import {apiEndpoint} from "../../api";

const Home = () => {
    const navigate = useNavigate();

    useEffect(() => {
        apiEndpoint('auth/verify').fetch()
            .then((res) => {
                if (res.data === 'Contractor')
                    navigate('/works')
                else if (res.data === 'Admin')
                    navigate('/admin')
                else if (res.data === 'Customer')
                    navigate('/dashboard')
                else
                    navigate('/login')
            })
            .catch(() => navigate('/login'))
    }, [navigate]);

    return (
        <>
            <div className="d-flex justify-content-center align-items-center" style={{height: "100vh"}}>
                <div className="spinner-border text-primary" role="status">
                    <span className="visually-hidden">Loading...</span>
                </div>
            </div>
        </>
    )
}

export default Home;