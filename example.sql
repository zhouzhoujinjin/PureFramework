/*
 Navicat Premium Data Transfer

 Source Server         : purecode-v7
 Source Server Type    : MySQL
 Source Server Version : 80100
 Source Host           : 81.70.71.241:33060
 Source Schema         : realestate_service_center

 Target Server Type    : MySQL
 Target Server Version : 80100
 File Encoding         : 65001

 Date: 10/04/2025 10:32:24
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for Core_Department
-- ----------------------------
DROP TABLE IF EXISTS `Core_Department`;
CREATE TABLE `Core_Department`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `Title` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `ParentId` bigint NULL DEFAULT NULL,
  `Seq` int NULL DEFAULT NULL,
  `CreatorId` bigint NULL DEFAULT NULL,
  `CreatedTime` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 13 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Core_DepartmentUser
-- ----------------------------
DROP TABLE IF EXISTS `Core_DepartmentUser`;
CREATE TABLE `Core_DepartmentUser`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `DepartmentId` bigint NULL DEFAULT NULL,
  `UserId` bigint NULL DEFAULT NULL,
  `Position` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Level` smallint NULL DEFAULT NULL,
  `IsUserMajorDepartment` tinyint(1) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 69 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Core_ProfileKey
-- ----------------------------
DROP TABLE IF EXISTS `Core_ProfileKey`;
CREATE TABLE `Core_ProfileKey`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `ValueSpaceId` bigint NULL DEFAULT NULL,
  `ProfileType` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `CategoryCode` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `IsSearchable` tinyint(1) NULL DEFAULT NULL,
  `IsBrief` tinyint(1) NULL DEFAULT NULL,
  `IsVisible` tinyint(1) NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 14 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Core_Role
-- ----------------------------
DROP TABLE IF EXISTS `Core_Role`;
CREATE TABLE `Core_Role`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `NormalizedName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Title` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `ConcurrencyStamp` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 5 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Core_RoleClaim
-- ----------------------------
DROP TABLE IF EXISTS `Core_RoleClaim`;
CREATE TABLE `Core_RoleClaim`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RoleId` bigint NULL DEFAULT NULL,
  `ClaimType` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `ClaimValue` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1751 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Core_Setting
-- ----------------------------
DROP TABLE IF EXISTS `Core_Setting`;
CREATE TABLE `Core_Setting`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `InstanceId` bigint NULL DEFAULT NULL,
  `ClassName` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `InstanceType` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 7 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Core_TreeNode
-- ----------------------------
DROP TABLE IF EXISTS `Core_TreeNode`;
CREATE TABLE `Core_TreeNode`  (
  `Id` bigint UNSIGNED NOT NULL AUTO_INCREMENT,
  `InstanceType` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '实体类型',
  `InstanceId` bigint UNSIGNED NULL DEFAULT NULL COMMENT '实体Id',
  `ParentId` bigint UNSIGNED NULL DEFAULT NULL COMMENT '父实体Id',
  `ParentIds` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL COMMENT '所有父亲节点的列表，使用逗号分割，首尾有逗号',
  `Seq` int NULL DEFAULT NULL COMMENT '同级中的顺序号',
  `ExtendData` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL COMMENT '扩展数据的json',
  PRIMARY KEY (`Id`) USING BTREE,
  INDEX `InstanceTypeIdx`(`InstanceType` ASC) USING BTREE,
  INDEX `ParentIdsIdx`(`InstanceType` ASC, `ParentIds` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 53 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Core_User
-- ----------------------------
DROP TABLE IF EXISTS `Core_User`;
CREATE TABLE `Core_User`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `UserName` varchar(60) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `NormalizedUserName` varchar(60) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Email` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `NormalizedEmail` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `EmailConfirmed` tinyint(1) NOT NULL,
  `Mobile` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `MobileConfirmed` tinyint(1) NULL DEFAULT NULL,
  `PasswordHash` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `SecurityStamp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `PhoneNumber` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `PhoneNumberConfirmed` tinyint(1) NOT NULL,
  `TwoFactorEnabled` tinyint(1) NOT NULL,
  `LockoutEnd` datetime NULL DEFAULT NULL,
  `LockoutEnabled` tinyint(1) NOT NULL,
  `AccessFailedCount` int NOT NULL,
  `IsDeleted` tinyint(1) NULL DEFAULT NULL,
  `IsVisible` tinyint(1) NULL DEFAULT NULL,
  `CreatedTime` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 36 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Core_UserClaim
-- ----------------------------
DROP TABLE IF EXISTS `Core_UserClaim`;
CREATE TABLE `Core_UserClaim`  (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` bigint NULL DEFAULT NULL,
  `ClaimType` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `ClaimValue` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Core_UserLog
-- ----------------------------
DROP TABLE IF EXISTS `Core_UserLog`;
CREATE TABLE `Core_UserLog`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `UserId` bigint NULL DEFAULT NULL,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Ip` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Method` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Device` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `UserAgent` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Data` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `CreatedTime` datetime NULL DEFAULT NULL,
  `Duration` double NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 12048 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Core_UserLogin
-- ----------------------------
DROP TABLE IF EXISTS `Core_UserLogin`;
CREATE TABLE `Core_UserLogin`  (
  `LoginProvider` varchar(60) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `ProviderKey` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `ProviderDisplayName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `UserId` bigint NULL DEFAULT NULL,
  PRIMARY KEY (`LoginProvider`, `ProviderKey`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Core_UserProfile
-- ----------------------------
DROP TABLE IF EXISTS `Core_UserProfile`;
CREATE TABLE `Core_UserProfile`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `UserId` bigint NULL DEFAULT NULL,
  `ProfileKeyId` bigint NULL DEFAULT NULL,
  `FullName` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 99 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Core_UserRole
-- ----------------------------
DROP TABLE IF EXISTS `Core_UserRole`;
CREATE TABLE `Core_UserRole`  (
  `RoleId` bigint NOT NULL,
  `UserId` bigint NOT NULL,
  PRIMARY KEY (`RoleId`, `UserId`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Core_UserToken
-- ----------------------------
DROP TABLE IF EXISTS `Core_UserToken`;
CREATE TABLE `Core_UserToken`  (
  `UserId` bigint NOT NULL,
  `LoginProvider` varchar(60) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Name` varchar(60) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Value` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`UserId`, `LoginProvider`, `Name`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for Core_ValueSpace
-- ----------------------------
DROP TABLE IF EXISTS `Core_ValueSpace`;
CREATE TABLE `Core_ValueSpace`  (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Title` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `ConfigureLevelCode` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Items` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 34 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

SET FOREIGN_KEY_CHECKS = 1;
