import {
  IndexPage,
  ProfilePage,
  UsersPage,
  UserPage,
  RolesPage,
  RolePage,
  ConfigPage as SystemConfigPage,
  MenuPage,
  ValueSpacePage,
  ValueSpacesPage,
  DepartmentsPage,
 
} from "./pages"


export const COMPONENTS = {
  Index: IndexPage,
  Profile: ProfilePage,

  // 用户
  Users: UsersPage,
  User: UserPage,
  Roles: RolesPage,
  Role: RolePage,
  Departments: DepartmentsPage,

  // 系统
  SystemConfig: SystemConfigPage,
  Menu: MenuPage,
  ValueSpaces: ValueSpacesPage,
  ValueSpace: ValueSpacePage,


  

}

export const paths = [
  { path: "/index", title: "首页", component: "Index" },
  {
    path: "/account/profile",
    exact: true,
    title: "个人设置",
    component: "Profile",
  },
  {
    path: "/departments",
    exact: true,
    title: "部门列表",
    component: "Departments",
  },
  {
    path: "/users/:id",
    title: "用户表单",
    component: "User",
  },
  {
    path: "/users",
    title: "用户列表",
    component: "Users",
  },
  {
    path: "/roles/:id",
    title: "角色表单",
    component: "Role",
  },
  {
    path: "/roles",
    title: "角色列表",
    component: "Roles",
  },
  {
    path: "/system/config",
    exact: true,
    title: "系统设置",
    component: "SystemConfig",
  },
  {
    path: "/system/menu",
    title: "菜单设置",
    component: "Menu",
  },
  {
    path: "/system/valueSpaces",
    exact: true,
    title: "字典列表",
    component: "ValueSpaces",
  },
  {
    path: "/system/valueSpaces/:name",
    exact: true,
    title: "字典表单",
    component: "ValueSpace",
  },
 


]
