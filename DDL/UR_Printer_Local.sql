-- --------------------------------------------------------
-- 호스트:                          127.0.0.1
-- 서버 버전:                        11.7.2-MariaDB - mariadb.org binary distribution
-- 서버 OS:                        Win64
-- HeidiSQL 버전:                  12.10.0.7000
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- ur_printer_local 데이터베이스 구조 내보내기
DROP DATABASE IF EXISTS `ur_printer_local`;
CREATE DATABASE IF NOT EXISTS `ur_printer_local` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci */;
USE `ur_printer_local`;

-- 테이블 ur_printer_local.order_history 구조 내보내기
DROP TABLE IF EXISTS `order_history`;
CREATE TABLE IF NOT EXISTS `order_history` (
  `order_number` int(11) NOT NULL COMMENT '발주번호',
  `order_date` datetime NOT NULL DEFAULT current_timestamp() COMMENT '발주일시',
  `usage` varchar(15) DEFAULT NULL COMMENT '용도',
  `liter` varchar(10) DEFAULT NULL COMMENT '리터',
  `order_amount` int(11) DEFAULT NULL COMMENT '발주량'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci COMMENT='발주내역\r\n';

-- 내보낼 데이터가 선택되어 있지 않습니다.

-- 테이블 ur_printer_local.production_data 구조 내보내기
DROP TABLE IF EXISTS `production_data`;
CREATE TABLE IF NOT EXISTS `production_data` (
  `serial` varchar(40) NOT NULL COMMENT '시리얼번호',
  `prd_date` varchar(25) DEFAULT NULL COMMENT '생산일(연-월-일)\r\nex)250404 : 25년0 4월 04일',
  `prd_line` varchar(10) DEFAULT NULL COMMENT '생산라인',
  `order_place` varchar(40) DEFAULT NULL COMMENT '발주처\r\n(ex:이천시)',
  `usage` varchar(15) DEFAULT NULL COMMENT '용도(일반,재사용,음식물)',
  `liter` varchar(5) DEFAULT NULL COMMENT '봉투 크기(리터)',
  `order_number` int(11) DEFAULT NULL COMMENT '발주번호',
  PRIMARY KEY (`serial`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci COMMENT='생산데이터 - (csv파일로도 저장되는 Data)';

-- 내보낼 데이터가 선택되어 있지 않습니다.

-- 테이블 ur_printer_local.work_planning 구조 내보내기
DROP TABLE IF EXISTS `work_planning`;
CREATE TABLE IF NOT EXISTS `work_planning` (
  `idx` int(11) NOT NULL AUTO_INCREMENT COMMENT 'PK',
  `order_id` int(11) NOT NULL DEFAULT 0 COMMENT '발주번호',
  `plan_date` datetime NOT NULL DEFAULT current_timestamp() COMMENT '작업계획량 할당 일시',
  `order_place` varchar(40) NOT NULL DEFAULT '' COMMENT '발주처',
  `prd_line` varchar(10) NOT NULL DEFAULT '' COMMENT '생산라인',
  `usage` varchar(15) NOT NULL DEFAULT '' COMMENT '용도(일반,재사용,음식물 등)',
  `liter` varchar(5) NOT NULL DEFAULT '' COMMENT '봉투크기(리터)',
  `planed_amount` int(11) NOT NULL DEFAULT 0 COMMENT '작업계획량',
  `st_serial` varchar(40) NOT NULL COMMENT '시작시리얼번호',
  `end_serial` varchar(40) NOT NULL COMMENT '끝시리얼번호',
  `is_done` varchar(1) NOT NULL DEFAULT 'N' COMMENT '작업계획량 완료 여부',
  PRIMARY KEY (`idx`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci COMMENT='작업계획 테이블';

-- 내보낼 데이터가 선택되어 있지 않습니다.

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
