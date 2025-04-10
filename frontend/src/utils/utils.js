export const getFriendlyUserName = (user, showFullName = true) => user.profiles.fullName ? `${user.profiles.fullName} (${user.userName})` : user.userName

export const loop = (array, callback, parent = null) => {
  array.forEach((item, index, arr) => {
    callback(item, index, arr, parent);

    if (item.children) {
      loop(item.children, callback, item);
    }
  });
};

export const requestFullScreen = (element) => {
  var requestMethod =
    element.requestFullScreen || //W3C
    element.webkitRequestFullScreen || //Chrome等
    element.mozRequestFullScreen || //FireFox
    element.msRequestFullScreen; //IE11
  if (requestMethod) {
    requestMethod.call(element);
  }
};

export const exitFullScreen = () => {
  var exitMethod =
    document.exitFullscreen || //W3C
    document.mozCancelFullScreen || //Chrome等
    document.webkitExitFullscreen || //FireFox
    document.webkitExitFullscreen; //IE11
  if (exitMethod) {
    exitMethod.call(document);
  }
};

export const parseToPossibleValue = (value) => {
  if (value === null || value === undefined) return "";
  if (!isNaN(+value)) return +value;
  if (value === "false") return false;
  if (value === "true") return true;
  if (value[0] === "[" || value[0] === "{") {
    try {
      return JSON.parse(value);
    } catch {
      // It's just a string, do nothing
    }
  }
  return value;
};
export const dotKeyToNested = (obj) => {
  var result = {},
    t,
    parts,
    part;
  for (var k in obj) {
    t = result;
    parts = k.split(".");
    var key = parts.pop();
    while (parts.length) {
      part = parts.shift();
      t = t[part] = t[part] || {};
    }
    t[key] = parseToPossibleValue(obj[k]);
  }
  return result;
};

export const nestedToDotKey = (obj, result = {}, keys = []) => {
  if (typeof obj === "object") {
    Object.keys(obj).forEach((k) => {
      if (k !== "response") {
        keys.push(k);
        nestedToDotKey(obj[k], result, keys);
        keys.pop();
      }
    });
  } else {
    if (keys !== "response") {
      result[keys.join(".")] = (obj && obj.toString()) || null;
    }
  }
  return result;
};

export const uuid = (len = 8, radix = 16) => {
  const chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".split(
    ""
  );
  const value = [];
  let i = 0;
  radix = radix || chars.length;
  if (len) {
    // Compact form
    for (i = 0; i < len; i++) value[i] = chars[0 | (Math.random() * radix)];
  } else {
    // rfc4122, version 4 form
    let r;
    // rfc4122 requires these characters
    /* eslint-disable-next-line */
    value[8] = value[13] = value[18] = value[23] = "-";
    value[14] = "4";
    // Fill in random data.  At i==19 set the high bits of clock sequence as
    // per rfc4122, sec. 4.1.5
    for (i = 0; i < 36; i++) {
      if (!value[i]) {
        r = 0 | (Math.random() * 16);
        value[i] = chars[i === 19 ? (r & 0x3) | 0x8 : r];
      }
    }
  }
  return value.join("");
};
