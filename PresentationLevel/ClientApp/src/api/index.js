import axios from 'axios'

export const apiEndpoint = (endpoint, form) => {
    let url = `api/${endpoint}`;
    const config = {
        headers: {
            Authorization: `Bearer ${localStorage.getItem('bearer_token')}`,
            'Content-Type': form ? 'multipart/form-data' : 'application/json',
        }
    };
    return {
        fetch: () => axios.get(url, config),
        fetchById: id => axios.get(url + `/${id}`, config),
        post: newRecord => axios.post(url, newRecord, config),
        put: updatedRecord => axios.put(url, updatedRecord, config),
        delete: () => axios.delete(url, config),
        deleteById: id => axios.delete(url + `/${id}`, config),
    }
}