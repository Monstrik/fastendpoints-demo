export type AdminUser = {
  id: string;
  login: string;
  firstName: string;
  lastName: string;
  age: number;
  fullName: string;
  role: string;
};

export type CreateAdminUserInput = {
  login: string;
  password: string;
  firstName: string;
  lastName: string;
  age: number;
  role: string;
};
