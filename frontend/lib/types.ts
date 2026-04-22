export type AdminUser = {
  id: string;
  login: string;
  firstName: string;
  lastName: string;
  fullName: string;
  role: string;
  status: string;
};

export type CreateAdminUserInput = {
  login: string;
  password: string;
  firstName: string;
  lastName: string;
  role: string;
};

export type PublicUserStatus = {
  login: string;
  fullName: string;
  status: string;
};
