import axios from 'axios';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8080/api'; // 5034 for local .NET development

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    withCredentials: true, // automatically send cookies
    headers: {
        'Content-Type': 'application/json',
    },
});

export default apiClient;