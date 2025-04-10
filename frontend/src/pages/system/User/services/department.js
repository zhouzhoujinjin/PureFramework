import request from '~/utils/request'
import { API } from '~/config'

export const GetDepartments = async (action) => {
  const data = await request.get(`${API}/admin/departments`)

  action([data])
}

export const GetDepartment = async (id, action) => {
  const data = await request.get(`${API}/departments/${id}`)
  // const extra = await request.get(`${API}/departments/${id}/extra`)
  action({ ...data})
  action([data])
}

export const AddDepartment = async (department, action) => {
  const data = await request.post(`${API}/admin/departments`, department)
  action(data)
}

export const UpdateDepartmentStruct = async (updateDepartmentIds, action) => {    
  const data = await request.post(`${API}/admin/departments/struct`, updateDepartmentIds)
  action && action(data)
}



export const UpdateDepartment = async (department, action) => {  
  const { id, ...rest } = department
  const data = await request.put(`${API}/admin/departments/${id}`, rest)
  action && action(data)
}

export const DeleteDepartment = async (departmentId, action) => {
  const data = await request.delete(`${API}/admin/departments/${departmentId}`)
  action && action(data)
}
