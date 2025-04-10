import React, { useEffect, useState } from 'react'
import { Checkbox, Input, Select } from 'antd'

const defaultPositions = [
  {
    label: '负责人',
    value: '负责人'
  },
  {
    label: '员工',
    value: '员工'
  },
  {
    label: '临时借调',
    value: '临时借调'
  }
]

const defaultLevels = [
  {
    label: '顶级',
    value: 0
  },
  {
    label: '一级',
    value: 1
  },
  {
    label: '二级',
    value: 2
  }
]

export const DepartmentUserInput = (props) => {
  const {
    value,
    onChange,
    isFirst,
    showPosition,
    showLevel,
    levels = defaultLevels,
    positions = defaultPositions
  } = props

  const [internalValue, setInternalValue] = useState({})

  const updateField = (field, value) => {
    const newValue = { ...internalValue, [field]: value }    
    setInternalValue(newValue)
    onChange && onChange(newValue)
  }

  useEffect(() => {
    if (value) setInternalValue(value)
  }, [value])

  return (
    <div className={`code-label-input ${isFirst ? 'first' : ''}`}>
      <Input.Group compact>
        <Input readOnly value={internalValue.profiles && internalValue.profiles.FullName}></Input>
        {showPosition && (
          <Select
            style={{ width: 120 }}
            options={positions}
            placeholder='职位'
            value={internalValue.position}
            onChange={(v) => updateField('position', v)}></Select>
        )}
        {showLevel && (
          <Select
            options={levels}
            style={{ width: 120 }}
            placeholder='职级'
            value={internalValue.level}
            onChange={(v) => updateField('level', v)}></Select>
        )}
      </Input.Group>
      <Checkbox
        checked={internalValue.isUserMajorDepartment}
        onChange={(v) => updateField('isUserMajorDepartment', v.target.checked)}>
        主部门
      </Checkbox>
    </div>
  )
}
