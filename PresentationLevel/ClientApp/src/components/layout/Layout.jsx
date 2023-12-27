import {ToastContainer, toast} from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import AppNavbar from "./AppNavbar";
import AppFooter from "./AppFooter";

export const launchError = (error) => {
    if (error?.response?.data?.message) {
        toast.error(error.response.data.message);
    }
    if (error?.response?.data) {
        toast.error(error.response.data);
    } else
        toast.error('Unknown Error');
}

export const launchSuccess = (message) => {
    toast.success(message.data);
}

const Layout = ({children}) => {
    return (
        <>
            <div className="d-flex flex-column min-vh-100">
                <AppNavbar/>
                <ToastContainer/>
                <div className="flex-grow-1">
                    {children}
                </div>
                <AppFooter/>
            </div>
        </>
    );
}

export default Layout;