import React, { useState, useEffect } from 'react'
import request from '../utils/request'
import { API } from '../config'
import { useHistory } from 'react-router-dom'
import { Spin } from 'antd'
import {token} from "~/utils/token";

export const GetAccountProfile = async (action, errorHandler) => {
  const cached = window.sessionStorage.getItem('profile')
  if (cached) return action && action(JSON.parse(cached))
  try {
    const data = await request.get(`${API}/account/profile`)
    window.sessionStorage.setItem('profile', JSON.stringify(data))
    action && action(data)
  } catch (error) {
    errorHandler && errorHandler(error)
  }
}

export const GetAccountPermissions = async (action, errorHandler) => {
  const cached = window.sessionStorage.getItem('permissions')
  if (cached) return action && action(JSON.parse(cached))
  try {
    const data = await request.get(`${API}/account/permissions`)
    window.sessionStorage.setItem('permissions', JSON.stringify(data))
    action && action(data)
  } catch (error) {
    errorHandler && errorHandler(error)
  }
}

export const ClearCached = () => {
  window.sessionStorage.removeItem('permissions')
  window.sessionStorage.removeItem('profile')
}

const AuthContext = React.createContext()

export const AuthProvider = ({ children, signInRoute }) => {
  const history = useHistory()
  const [updated, setUpdated] = useState(false)
  const [state, setState] = useState({
    user: {},
    status: 'pending',
    refresh: () => {
      ClearCached()
      setUpdated(!updated)
    }
  })
  useEffect(() => {
    if (token.get()) {
      GetAccountProfile(
        (profiles) =>
          setState((u) => ({
            ...u,
            status: "success",
            user: {
              ...u.user,
              ...profiles,
            },
          })),
        (error) => {
          setState({
            status: "error",
          });
        }
      );
      GetAccountPermissions(
        (permissions) =>
          setState((u) => ({
            ...u,
            status: "success",
            user: {
              ...u.user,
              permissions,
            },
          })),
        (error) => {
          setState({
            status: "error",
          });
        }
      );
    } else {
      history.push(signInRoute);
    }
  }, [updated]);
  if (state.status === "error") {
    history.push(signInRoute);
  }
  return (
    <AuthContext.Provider value={{ state, setState }}>
      {state.status !== 'success' ? <Spin /> : children}
    </AuthContext.Provider>
  )
}

export const useAuth = () => {
  const { state, setState } = React.useContext(AuthContext)
  const isPending = state.status === 'pending'
  const isError = state.status === 'error'
  const isSuccess = state.status === 'success'
  const isAuthenticated = state.user && isSuccess

  const updateProfile = (user) => {
    window.sessionStorage.setItem('profile', JSON.stringify(user))
    setState((u) => ({
      ...u,
      user
    }))
  }

  const checkProfile = (key, value) => {
    return state.user && state.user.profile[key] === value
  }
  const checkPermission = (perm) => {
    if (typeof perm === "string") {
      return (
        state.user &&
        state.user.permissions &&
        state.user.permissions.length > 0 &&
        state.user.permissions.indexOf(perm) > -1
      );
    }
    if (Array.isArray(perm)) {
      return (
        state.user &&
        state.user.permissions &&
        state.user.permissions.length > 0 &&
        state.user.permissions.filter(Set.prototype.has, new Set(perm)).length > 0
      )
    }
  }

  return {
    ...state,
    isPending,
    isError,
    isSuccess,
    isAuthenticated,
    updateProfile,
    checkPermission,
    checkProfile
  }
}
