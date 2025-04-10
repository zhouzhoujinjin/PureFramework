export const Flex = ({ align, style, gap, justify, direction, children }) => {
  return (
    <div
      style={{
        ...style,
        display: "flex",
        gap,
        justifyContent: justify || "center",
        alignItems: align || "center",
        flexDirection: direction || "row",
      }}
    >
      {children}
    </div>
  );
};
