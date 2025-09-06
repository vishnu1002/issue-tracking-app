export type Role = 'User' | 'Rep' | 'Admin';

export interface UserModel {
  id: string;
  name: string;
  email: string;
  role: Role;
  createdAt?: string;
}
