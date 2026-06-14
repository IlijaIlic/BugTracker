export interface AuthResponseDto{
    token: string;
    email: string;
    username: string;
    role: string;
}

export interface RegisterModel{
    Username: string;
    Email: string;
    Password: string;
    reppassword: string;
    Role: string;
}

export interface LoginModel{
    Email: string;
    Password: string;
}