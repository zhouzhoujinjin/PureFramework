import React, { useEffect, useRef, useState } from "react";
import { Input } from "antd";
import { default as SketchPicker } from "react-color/lib/Sketch";
import ReactDOM from "react-dom";

export const ColorPicker = ({ onChange, value, size }) => {
  const [displayColorPicker, setDisplayColorPicker] = useState({
    value: false,
  });
  const [internalValue, setInternalValue] = useState("#ffffff");

  const colorRef = useRef(null);
  const [postion, setPosition] = useState({ top: 0, left: 0 });

  useEffect(() => {
    setInternalValue(value);
  }, [value]);

  useEffect(() => {
    if (colorRef.current) {
      setPosition({
        top: 20,
        right: 0,
        left: -196,
      });
    }
  }, []);

  const handleClick = () => {
    setDisplayColorPicker({ value: true });
  };
  const handleClose = (e) => {
    e.stopPropagation();
    setDisplayColorPicker({ value: false });
  };
  const handleChange = (color) => {
    let str = color.hex;
    if (color.rgb.a !== 1) {
      str = `rgba(${color.rgb.r}, ${color.rgb.g}, ${color.rgb.b}, ${color.rgb.a})`;
    }
    setInternalValue(str);
    setDisplayColorPicker({ value: true });
  };
  const handleChangeComplete = (color) => {
    let str = color.hex;
    if (color.rgb.a !== 1) {
      str = `rgba(${color.rgb.r}, ${color.rgb.g}, ${color.rgb.b}, ${color.rgb.a})`;
    }
    onChange && onChange(str);
  };

  const styles = {
    color: {
      width: size === "small" ? "12px" : "16px",
      height: size === "small" ? "12px" : "16px",
      borderRadius: "2px",
      background: internalValue,
    },
    swatch: {
      margin: size === "small" ? "4px 0 -2px" : "6px 0 0",
      background: "#fff",
      borderRadius: "2px",
      border: "1px solid #0000001A",
      display: "inline-block",
      cursor: "pointer",
      postion: "relative",
    },
    popover: {
      position: "absolute",
      zIndex: 100,
      left: postion.left,
      top: postion.top,
    },
    cover: {
      position: "fixed",
      top: "0px",
      right: "0px",
      bottom: "0px",
      left: "0px",
    },
    wrapper: {
      position: "inherit",
    },
  };

  const handleInputChange = (e) => {
    setInternalValue(e.target.value);
    onChange && onChange(e.target.value);
  };

  const picker = (
    <div style={styles.popover}>
      <div style={styles.cover} onClick={handleClose}></div>
      <div style={styles.wrapper}>
        <SketchPicker
          color={internalValue}
          onChange={handleChange}
          onChangeComplete={handleChangeComplete}
        />
      </div>
    </div>
  );

  return (
    <Input
      size={size}
      onFocus={() => setDisplayColorPicker({ value: true })}
      onChange={handleInputChange}
      value={internalValue}
      addonAfter={
        <div style={styles.swatch} onClick={handleClick} ref={colorRef}>
          <div style={styles.color} />
          {displayColorPicker.value
            ? ReactDOM.createPortal(picker, colorRef.current)
            : null}
        </div>
      }
    />
  );
};

export default ColorPicker;
