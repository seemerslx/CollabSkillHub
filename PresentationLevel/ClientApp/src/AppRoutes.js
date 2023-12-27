import Login from "./pages/auth/Login";
import Register from "./pages/auth/Register";
import ClientPage from "./pages/home/ClientPage";
import AddWorkForm from "./pages/forms/AddWorkForm";
import Home from "./pages/home/Home";
import ContractorPage from "./pages/home/ContractorPage";
import RequestPage from "./pages/requests/RequestPage";
import Chats from "./pages/chats/Chats";

const AppRoutes = [
    {
        index: true,
        element: <Home/>
    },
    {
        path: '/login',
        element: <Login/>
    },
    {
        path: '/register/:accountType?',
        element: <Register/>
    },
    {
        path: '/dashboard',
        element: <ClientPage/>
    },
    {
        path: '/dashboard/form/:id?',
        element: <AddWorkForm/>
    },
    {
        path: '/works',
        element: <ContractorPage/>
    },
    {
        path: '/requests',
        element: <RequestPage/>
    },
    {
        path: '/chats/:id?',
        element: <Chats/>
    },
];

export default AppRoutes;
