export default [
  {
    url: "/api/config",
    method: "get",
    response: ({ body }) => {
      return {
        code: 0,
        message: null,
        data: {
          openStartDate: "1901-01-01T00:00:00",
          openEndDate: "2023-12-31T00:00:00",
          siteName: "新锐亚",
          siteRoot: null,
          defaultAvatarSize: {
            width: 40,
            height: 40,
          },
          siteUnderMaintenance: false,
          beiAnNo: null,
        },
      };
    },
  },
  {
    url: "/api/admin/valueSpaces",
    method: "get",
    response: ({ body }) => {
      return {
        total: 33,
        page: 1,
        hasMore: false,
        code: 0,
        message: null,
        data: [
          {
            conditions: "^\\/(.+)*$",
            regex: { pattern: "^\\/(.+)*$", options: 0 },
            name: "absoluteUrl",
            title: "站内网址",
            valueSpaceType: "Regex",
            configureLevel: "System",
            valueType: "string",
          },
          {
            conditions: {
              "000000": "未知",
              110000: "北京市",
              110101: "东城区",
              110102: "西城区",
              110105: "朝阳区",
              110106: "丰台区",
            },
            name: "administrativeDivision",
            title: "行政区划",
            valueSpaceType: "Code",
            configureLevel: "Configurable",
            valueType: "string",
          },
          {
            conditions: "^True|False$",
            regex: { pattern: "^True|False$", options: 0 },
            name: "boolean",
            title: "布尔",
            valueSpaceType: "Regex",
            configureLevel: "System",
            valueType: "bool",
          },
          {
            conditions: "^[\\u4e00-\\u9fa5_a-zA-Z]+$",
            regex: { pattern: "^[\\u4e00-\\u9fa5_a-zA-Z]+$", options: 0 },
            name: "chineseAndEnglish",
            title: "中英文",
            valueSpaceType: "Regex",
            configureLevel: "System",
            valueType: "string",
          },
          {
            conditions: "[\\d]{4}[\\.\\-][\\d]{1,2}[\\.\\-][\\d]{1,2}",
            regex: {
              pattern: "[\\d]{4}[\\.\\-][\\d]{1,2}[\\.\\-][\\d]{1,2}",
              options: 0,
            },
            name: "date",
            title: "日期",
            valueSpaceType: "Regex",
            configureLevel: "System",
            valueType: "string",
          },
          {
            conditions: "^[\\w-\\.]+@[\\w-]+(\\.[a-zA-Z]+)+$",
            regex: {
              pattern: "^[\\w-\\.]+@[\\w-]+(\\.[a-zA-Z]+)+$",
              options: 0,
            },
            name: "email",
            title: "邮箱",
            valueSpaceType: "Regex",
            configureLevel: "System",
            valueType: "string",
          },
          {
            conditions: "^[a-zA-Z]+$",
            regex: { pattern: "^[a-zA-Z]+$", options: 0 },
            name: "english",
            title: "英文",
            valueSpaceType: "Regex",
            configureLevel: "System",
            valueType: "string",
          },
        ],
      };
    },
  },
  {
    url: "/api/valueSpaces/administrativeDivision",
    method: "get",
    response: ({ body }) => {
      return {
        code: 0,
        message: null,
        data: {
          conditions: [
            { "code": "000000", "title": "未知" },
            { "code": "110000", "title": "北京市" },
            { "code": "110101", "title": "东城区" },
            { "code": "110102", "title": "西城区" },
            { "code": "110105", "title": "朝阳区" },
            { "code": "110106", "title": "丰台区" }
          ],
          name: "administrativeDivision",
          title: "行政区划",
          valueSpaceType: "Code",
          configureLevel: "Configurable",
          valueType: "string",
        },
      };
    },
  },
  {
    url: "/api/valueSpaces/controlTypeIcon",
    method: "get",
    response: ({ body }) => {
      return {
        code: 0,
        message: null,
        data: {
          conditions: [
            { "code": "group", "title": "list-tree" },
            { "code": "list", "title": "square-list" },
            { "code": "select", "title": "ballot-check" },
            { "code": "input", "title": "input-pipe" },
            { "code": "textarea", "title": "align-left" },
            { "code": "date", "title": "calendar-days" },
            { "code": "date-range", "title": "calendar-week" },
            { "code": "time", "title": "calendar-clock" },
            { "code": "radio", "title": "list-radio" },
            { "code": "checkbox", "title": "list-check" },
            { "code": "switch", "title": "toggle-on" },
            { "code": "map-location", "title": "map" }
          ],
          name: "controlTypeIcon",
          title: "控件类型图标",
          valueSpaceType: "Code",
          configureLevel: "Configurable",
          valueType: "string",
        },
      };
    }
  },
  {
    url: "/api/valueSpaces/controlType",
    method: "get",
    response: ({ body }) => {
      return {
        code: 0,
        message: null,
        data: {
          conditions: [
            { "code": "group", "title": "字段对象" },
            { "code": "list", "title": "字段数组" },
            { "code": "select", "title": "列表项" },
            { "code": "input", "title": "文本框" },
            { "code": "textarea", "title": "多行文本框" },
            { "code": "date", "title": "日期选择器" },
            { "code": "date-range", "title": "日期范围选择器" },
            { "code": "time", "title": "时间选择器" },
            { "code": "radio", "title": "单选框" },
            { "code": "checkbox", "title": "多选框" },
            { "code": "switch", "title": "开关" },
            { "code": "map-location", "title": "地图选择器" }
          ],
          name: "controlType",
          title: "控件类型",
          valueSpaceType: "Code",
          configureLevel: "Configurable",
          valueType: "string",
        },
      };
    },
  },
  {
    url: "/api/admin/system/menu",
    method: "get",
    response: () => ({
      code: 0,
      message: null,
      data: [
        {
          type: "route",
          path: "/index",
          title: "数据看板",
          iconName: "dashboard",
          hideChildren: false,
          hidden: false,
        },
        {
          type: "route",
          path: "/devices",
          title: "设备管理",
          iconName: "ethernet",
          hideChildren: true,
          hidden: false,
          children: [
            { type: "route", path: "/devices/:key", title: "设备表单" },
          ],
        },
        {
          type: "route",
          path: "/settings/index",
          title: "配置管理",
          iconName: "scroll",
          hideChildren: false,
          hidden: false,
          children: [
            {
              type: "route",
              path: "/settings/templates",
              title: "配置模板",
              children: [
                {
                  type: "route",
                  path: "/settings/templates/:key",
                  title: "模板表单",
                },
              ],
              hideChildren: true,
              hidden: false,
            },
            {
              type: "route",
              path: "/settings/fields",
              title: "字段管理",
            },
          ],
        },
        {
          type: "route",
          path: "/qrcode/index",
          title: "二维码管理",
          iconName: "qrcode",
          hideChildren: true,
          hidden: false,
          children: [
            {
              type: "route",
              path: "/qrcode/create",
              title: "新建二维码批次",
            },
            {
              type: "route",
              path: "/qrcode/:key",
              title: "查看二维码批次",
            },
          ],
        },
        {
          type: "route",
          path: "/users/index",
          title: "用户管理",
          iconName: "users",
          hideChildren: false,
          hidden: false,
          children: [
            {
              type: "route",
              path: "/users",
              title: "用户",
              hideChildren: true,
              hidden: false,
              children: [
                {
                  type: "route",
                  path: "/users/:id",
                  title: "用户详情",
                  hideChildren: false,
                  hidden: false,
                },
                {
                  type: "action",
                  path: "user:add",
                  title: "允许添加用户",
                  hideChildren: true,
                  hidden: false,
                },
              ],
            },
            {
              type: "route",
              path: "/roles",
              title: "角色",
              hideChildren: true,
              hidden: false,
              children: [
                {
                  type: "route",
                  path: "/roles/:id",
                  title: "角色详情",
                  hideChildren: false,
                  hidden: false,
                },
              ],
            },
            {
              type: "route",
              path: "/departments",
              title: "部门",
              hideChildren: false,
              hidden: false,
            },
          ],
        },
        {
          type: "route",
          path: "/tenants",
          title: "租户管理",
          iconName: "house-user",
          hideChildren: true,
          hidden: false,
          children: [
            { type: "route", path: "/tenants/:id", title: "租户表单" },
          ],
        },
        {
          type: "route",
          path: "/system",
          title: "系统管理",
          iconName: "cogs",
          hideChildren: false,
          hidden: false,
          children: [
            {
              type: "route",
              path: "/system/config",
              title: "系统设置",
              hideChildren: false,
              hidden: false,
            },
            {
              type: "route",
              path: "/system/menu",
              title: "菜单设置",
              hideChildren: false,
              hidden: false,
            },
            {
              type: "route",
              path: "/system/valueSpaces",
              title: "值空间",
              hideChildren: true,
              hidden: false,
              children: [
                {
                  type: "route",
                  path: "/system/valueSpaces/:name",
                  title: "值空间",
                  hideChildren: false,
                  hidden: false,
                },
              ],
            },
            {
              type: "route",
              path: "/system/frontendPermission",
              title: "前端权限",
              hideChildren: false,
              hidden: false,
            },
          ],
        },
      ],
    }),
  },
];
