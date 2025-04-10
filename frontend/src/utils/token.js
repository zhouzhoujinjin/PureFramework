import { jwtDecode } from "jwt-decode";

export const STORAGE_TOKEN_NAME = "TOKEN";
export const STORAGE_CURRENT_USER = "CURRENT_USER";

let user;

export const token = {
  get() {
    return (
      localStorage.getItem(STORAGE_TOKEN_NAME) ||
      sessionStorage.getItem(STORAGE_TOKEN_NAME)
    );
  },
  getUser() {
    if (!user) {
      user = JSON.parse(localStorage.getItem(STORAGE_CURRENT_USER));
    }
    return user;
  },
  save(token) {
    localStorage.setItem(
      STORAGE_CURRENT_USER,
      JSON.stringify(jwtDecode(token.accessToken))
    );
    localStorage.setItem(STORAGE_TOKEN_NAME, token.accessToken);
  },
  remove() {
    localStorage.removeItem(STORAGE_TOKEN_NAME);
    localStorage.removeItem(STORAGE_CURRENT_USER);
    user = null;
  },
};
