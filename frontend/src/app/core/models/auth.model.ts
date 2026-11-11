export interface LoginRequest {
  email: string;
  password: string;
}
export interface RegisterRequest {
  username: string;
  password: string;
  fullName: string;
  email: string;
  phoneNumber: string;
}
export interface AuthResponse {
  token: string;
  userId: string;
  username: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  role: string;
  expiresAt: string;
}
export interface UserInfo {
  id: string;
  username: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}
