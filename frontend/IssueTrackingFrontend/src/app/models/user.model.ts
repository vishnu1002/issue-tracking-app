export type Role = 'User' | 'Representative' | 'Admin';

export interface UserModel {
  id: string;
  name: string;
  email: string;
  role: Role;
  createdAt?: string;
}
