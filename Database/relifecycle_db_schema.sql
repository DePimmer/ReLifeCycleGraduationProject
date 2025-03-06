-- MySQL dump 10.13  Distrib 8.0.38, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: relifecycle_db_test
-- ------------------------------------------------------
-- Server version	9.1.0

CREATE DATABASE  IF NOT EXISTS `relifecycle_db` /*!40100 DEFAULT CHARACTER SET utf8mb3 */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `relifecycle_db`;


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `archicalc_db`
--

DROP TABLE IF EXISTS `archicalc_db`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `archicalc_db` (
  `archicalc_id` varchar(10) NOT NULL,
  `archicalc_product_name` text,
  `material_costs_per_unit` double DEFAULT NULL,
  `labour_costs_per_unit` double DEFAULT NULL,
  `source` text,
  PRIMARY KEY (`archicalc_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `archicalc_db`
--

LOCK TABLES `archicalc_db` WRITE;
/*!40000 ALTER TABLE `archicalc_db` DISABLE KEYS */;
INSERT INTO `archicalc_db` VALUES ('CALC_001','EXAMPLE Concrete floor',50,45,'Archicalc');
/*!40000 ALTER TABLE `archicalc_db` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `classification_junction_table`
--

DROP TABLE IF EXISTS `classification_junction_table`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `classification_junction_table` (
  `nmd_id` varchar(10) NOT NULL,
  `nl_sfb_code` varchar(10) NOT NULL,
  UNIQUE KEY `unique_combination_id` (`nmd_id`,`nl_sfb_code`),
  KEY `nl_sfb_code_idx` (`nl_sfb_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `classification_junction_table`
--

LOCK TABLES `classification_junction_table` WRITE;
/*!40000 ALTER TABLE `classification_junction_table` DISABLE KEYS */;
INSERT INTO `classification_junction_table` VALUES ('nmd_001','13A');
/*!40000 ALTER TABLE `classification_junction_table` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Temporary view structure for view `classification_mapping`
--

DROP TABLE IF EXISTS `classification_mapping`;
/*!50001 DROP VIEW IF EXISTS `classification_mapping`*/;
SET @saved_cs_client     = @@character_set_client;
/*!50503 SET character_set_client = utf8mb4 */;
/*!50001 CREATE VIEW `classification_mapping` AS SELECT 
 1 AS `nl_sfb_code`,
 1 AS `nl_sfb_name_nl`,
 1 AS `nl_sfb_name_en`,
 1 AS `nmd_id`,
 1 AS `nibe_id`,
 1 AS `archicalc_id`*/;
SET character_set_client = @saved_cs_client;

--
-- Table structure for table `classification_table`
--

DROP TABLE IF EXISTS `classification_table`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `classification_table` (
  `nl_sfb_code` varchar(10) NOT NULL,
  `nl_sfb_name_nl` text,
  `nl_sfb_name_en` text,
  PRIMARY KEY (`nl_sfb_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `classification_table`
--

LOCK TABLES `classification_table` WRITE;
/*!40000 ALTER TABLE `classification_table` DISABLE KEYS */;
INSERT INTO `classification_table` VALUES ('11','Bodemvoorzieningen','Ground'),('13A','Vloeren op grondslag - Constructie','Floors on the ground - Structure'),('13B','Vloeren op grondslag - Isolatie','Floors on the ground - Insulation'),('16','Funderingsconstructies','Foundation'),('17','Paalfunderingen','Pile foundations'),('21A','Buitenwanden - Constructie','External walls - Structure'),('21B','Buitenwanden - Isolatie','External walls - Insulation'),('22A','Binnenwanden - Constructie','Internal walls - Structure'),('22B','Binnenwanden - Isolatie','Internal walls - Insulation'),('23A','Vloeren - Constructie','Floors - Structure'),('23B','Vloeren - Isolatie','Floors - Insulation'),('24','Trappen en hellingen','Stairs and ramps'),('27A','Daken - Constructie','Roofs - Structure'),('27B','Daken - Isolatie','Roofs - Insulation'),('28A','Hoofddraagconstructies - Algemeen','Load-bearing structures - General'),('28B','Hoofddraagconstructies - Bewapening','Load-bearing structures - Reinforcement'),('31A','Buitenwandopeningen - Ramen (kozijnen)','External wall openings - Windows (frames)'),('31B','Buitenwandopeningen - Ramen (beglazing)','External wall openings - Windows (glazing)'),('31C','Buitenwandopeningen - Deuren','External wall openings - Doors'),('32A','Binnenwandopeningen - Ramen (kozijnen)','Internal wall openings - Windows (frames)'),('32B','Binnenwandopeningen - Ramen (beglazing)','Internal wall openings - Windows (glazing)'),('32C','Binnenwandopeningen - Deuren','Internal wall openings - Doors'),('33A','Vloeropeningen - Ramen (kozijnen)','Floor openings - Windows (frames)'),('33B','Vloeropeningen - Ramen (beglazing)','Floor openings - Windows (glazing)'),('34','Balustrades en leuningen','Stair balustrades and handrails'),('37A','Dakopeningen - Ramen (kozijnen)','Roof openings - Windows (frames)'),('37B','Dakopeningen - Ramen (beglazing)','Roof openings - Windows (glazing)'),('41','Buitenwandafwerkingen','External wall finishes'),('42','Binnenwandafwerkingen','Internal wall finishes'),('43','Vloerafwerkingen','Floor finishes'),('44','Trap- en hellingafwerkingen','Stair and ramp finishes'),('45','Plafondafwerkingen','Ceiling finishes'),('47','Dakafwerkingen','Roof finishes');
/*!40000 ALTER TABLE `classification_table` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `nibe_db`
--

DROP TABLE IF EXISTS `nibe_db`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `nibe_db` (
  `nibe_id` varchar(10) NOT NULL,
  `nibe_product_name` text,
  `mass_per_unit` double DEFAULT NULL,
  `csc` double DEFAULT NULL,
  `%_new` int DEFAULT NULL,
  `%_biobased` int DEFAULT NULL,
  `%_recycled` int DEFAULT NULL,
  `%_reused` int DEFAULT NULL,
  `%_landfill` int DEFAULT NULL,
  `%_burning` int DEFAULT NULL,
  `%_recycling` int DEFAULT NULL,
  `%_reusing` int DEFAULT NULL,
  `dp` int DEFAULT NULL,
  `mci` int DEFAULT NULL,
  `pci` int DEFAULT NULL,
  PRIMARY KEY (`nibe_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `nibe_db`
--

LOCK TABLES `nibe_db` WRITE;
/*!40000 ALTER TABLE `nibe_db` DISABLE KEYS */;
INSERT INTO `nibe_db` VALUES ('NIBE_001','EXAMPLE Concrete floor',450,0,100,0,0,0,10,0,90,0,5,35,30);
/*!40000 ALTER TABLE `nibe_db` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `nmd_db`
--

DROP TABLE IF EXISTS `nmd_db`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `nmd_db` (
  `nmd_id` varchar(10) NOT NULL,
  `nibe_id` varchar(10) NOT NULL,
  `archicalc_id` varchar(10) NOT NULL,
  `nmd_category` varchar(10) DEFAULT NULL,
  `product_description_nl` varchar(255) DEFAULT NULL,
  `product_description_en` varchar(255) DEFAULT NULL,
  `unit` varchar(10) DEFAULT NULL,
  `thickness` double DEFAULT NULL,
  `technical_lifespan` double DEFAULT NULL,
  `gwp_a1_a2_a3` double DEFAULT NULL,
  `gwp_a4` double DEFAULT NULL,
  `gwp_a5` double DEFAULT NULL,
  `gwp_b1` double DEFAULT NULL,
  `gwp_b2` double DEFAULT NULL,
  `gwp_b3` double DEFAULT NULL,
  `gwp_b4` double DEFAULT NULL,
  `gwp_b5` double DEFAULT NULL,
  `gwp_c1` double DEFAULT NULL,
  `gwp_c2` double DEFAULT NULL,
  `gwp_c3` double DEFAULT NULL,
  `gwp_c4` double DEFAULT NULL,
  `gwp_d` double DEFAULT NULL,
  `mki_a1_a2_a3` double DEFAULT NULL,
  `mki_a4` double DEFAULT NULL,
  `mki_a5` double DEFAULT NULL,
  `mki_b1` double DEFAULT NULL,
  `mki_b2` double DEFAULT NULL,
  `mki_b3` double DEFAULT NULL,
  `mki_b4` double DEFAULT NULL,
  `mki_b5` double DEFAULT NULL,
  `mki_c1` double DEFAULT NULL,
  `mki_c2` double DEFAULT NULL,
  `mki_c3` double DEFAULT NULL,
  `mki_c4` double DEFAULT NULL,
  `mki_d` double DEFAULT NULL,
  PRIMARY KEY (`nmd_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `nmd_db`
--

LOCK TABLES `nmd_db` WRITE;
/*!40000 ALTER TABLE `nmd_db` DISABLE KEYS */;
INSERT INTO `nmd_db` VALUES ('nmd_001','NIBE_001','CALC_001','2','VOORBEELD Betonnen vloer','EXAMPLE Concrete floor','m2',0.25,999,50,2,0.5,0,0,0,0,0,6,7,1.5,0.03,-3,13,0.2,0.5,0,0,0,0,0,4,2,0.2,0.01,-0.7);
/*!40000 ALTER TABLE `nmd_db` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Temporary view structure for view `relifecycle_joined_db`
--

DROP TABLE IF EXISTS `relifecycle_joined_db`;
/*!50001 DROP VIEW IF EXISTS `relifecycle_joined_db`*/;
SET @saved_cs_client     = @@character_set_client;
/*!50503 SET character_set_client = utf8mb4 */;
/*!50001 CREATE VIEW `relifecycle_joined_db` AS SELECT 
 1 AS `nl_sfb_code`,
 1 AS `nl_sfb_name_nl`,
 1 AS `nl_sfb_name_en`,
 1 AS `nmd_id`,
 1 AS `nibe_id`,
 1 AS `archicalc_id`,
 1 AS `nmd_category`,
 1 AS `product_description_nl`,
 1 AS `product_description_en`,
 1 AS `unit`,
 1 AS `thickness`,
 1 AS `technical_lifespan`,
 1 AS `gwp_a1_a2_a3`,
 1 AS `gwp_a4`,
 1 AS `gwp_a5`,
 1 AS `gwp_b1`,
 1 AS `gwp_b2`,
 1 AS `gwp_b3`,
 1 AS `gwp_b4`,
 1 AS `gwp_b5`,
 1 AS `gwp_c1`,
 1 AS `gwp_c2`,
 1 AS `gwp_c3`,
 1 AS `gwp_c4`,
 1 AS `gwp_d`,
 1 AS `mki_a1_a2_a3`,
 1 AS `mki_a4`,
 1 AS `mki_a5`,
 1 AS `mki_b1`,
 1 AS `mki_b2`,
 1 AS `mki_b3`,
 1 AS `mki_b4`,
 1 AS `mki_b5`,
 1 AS `mki_c1`,
 1 AS `mki_c2`,
 1 AS `mki_c3`,
 1 AS `mki_c4`,
 1 AS `mki_d`,
 1 AS `nibe_product_name`,
 1 AS `mass_per_unit`,
 1 AS `csc`,
 1 AS `%_new`,
 1 AS `%_biobased`,
 1 AS `%_recycled`,
 1 AS `%_reused`,
 1 AS `%_landfill`,
 1 AS `%_burning`,
 1 AS `%_recycling`,
 1 AS `%_reusing`,
 1 AS `dp`,
 1 AS `mci`,
 1 AS `pci`,
 1 AS `archicalc_product_name`,
 1 AS `material_costs_per_unit`,
 1 AS `labour_costs_per_unit`,
 1 AS `source`*/;
SET character_set_client = @saved_cs_client;

--
-- Final view structure for view `classification_mapping`
--

/*!50001 DROP VIEW IF EXISTS `classification_mapping`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8mb4 */;
/*!50001 SET character_set_results     = utf8mb4 */;
/*!50001 SET collation_connection      = utf8mb4_0900_ai_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50013 DEFINER=`root`@`localhost` SQL SECURITY DEFINER */
/*!50001 VIEW `classification_mapping` AS select `c`.`nl_sfb_code` AS `nl_sfb_code`,`c`.`nl_sfb_name_nl` AS `nl_sfb_name_nl`,`c`.`nl_sfb_name_en` AS `nl_sfb_name_en`,`n`.`nmd_id` AS `nmd_id`,`n`.`nibe_id` AS `nibe_id`,`n`.`archicalc_id` AS `archicalc_id` from ((((`classification_table` `c` join `classification_junction_table` `j` on((`c`.`nl_sfb_code` = `j`.`nl_sfb_code`))) join `nmd_db` `n` on((`j`.`nmd_id` = `n`.`nmd_id`))) left join `nibe_db` `ni` on((`n`.`nibe_id` = `ni`.`nibe_id`))) left join `archicalc_db` `ar` on((`n`.`archicalc_id` = `ar`.`archicalc_id`))) order by `c`.`nl_sfb_code`,`n`.`nmd_id` */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;

--
-- Final view structure for view `relifecycle_joined_db`
--

/*!50001 DROP VIEW IF EXISTS `relifecycle_joined_db`*/;
/*!50001 SET @saved_cs_client          = @@character_set_client */;
/*!50001 SET @saved_cs_results         = @@character_set_results */;
/*!50001 SET @saved_col_connection     = @@collation_connection */;
/*!50001 SET character_set_client      = utf8mb4 */;
/*!50001 SET character_set_results     = utf8mb4 */;
/*!50001 SET collation_connection      = utf8mb4_0900_ai_ci */;
/*!50001 CREATE ALGORITHM=UNDEFINED */
/*!50013 DEFINER=`root`@`localhost` SQL SECURITY DEFINER */
/*!50001 VIEW `relifecycle_joined_db` AS select `c`.`nl_sfb_code` AS `nl_sfb_code`,`c`.`nl_sfb_name_nl` AS `nl_sfb_name_nl`,`c`.`nl_sfb_name_en` AS `nl_sfb_name_en`,`n`.`nmd_id` AS `nmd_id`,`n`.`nibe_id` AS `nibe_id`,`n`.`archicalc_id` AS `archicalc_id`,`n`.`nmd_category` AS `nmd_category`,`n`.`product_description_nl` AS `product_description_nl`,`n`.`product_description_en` AS `product_description_en`,`n`.`unit` AS `unit`,`n`.`thickness` AS `thickness`,`n`.`technical_lifespan` AS `technical_lifespan`,`n`.`gwp_a1_a2_a3` AS `gwp_a1_a2_a3`,`n`.`gwp_a4` AS `gwp_a4`,`n`.`gwp_a5` AS `gwp_a5`,`n`.`gwp_b1` AS `gwp_b1`,`n`.`gwp_b2` AS `gwp_b2`,`n`.`gwp_b3` AS `gwp_b3`,`n`.`gwp_b4` AS `gwp_b4`,`n`.`gwp_b5` AS `gwp_b5`,`n`.`gwp_c1` AS `gwp_c1`,`n`.`gwp_c2` AS `gwp_c2`,`n`.`gwp_c3` AS `gwp_c3`,`n`.`gwp_c4` AS `gwp_c4`,`n`.`gwp_d` AS `gwp_d`,`n`.`mki_a1_a2_a3` AS `mki_a1_a2_a3`,`n`.`mki_a4` AS `mki_a4`,`n`.`mki_a5` AS `mki_a5`,`n`.`mki_b1` AS `mki_b1`,`n`.`mki_b2` AS `mki_b2`,`n`.`mki_b3` AS `mki_b3`,`n`.`mki_b4` AS `mki_b4`,`n`.`mki_b5` AS `mki_b5`,`n`.`mki_c1` AS `mki_c1`,`n`.`mki_c2` AS `mki_c2`,`n`.`mki_c3` AS `mki_c3`,`n`.`mki_c4` AS `mki_c4`,`n`.`mki_d` AS `mki_d`,`ni`.`nibe_product_name` AS `nibe_product_name`,`ni`.`mass_per_unit` AS `mass_per_unit`,`ni`.`csc` AS `csc`,`ni`.`%_new` AS `%_new`,`ni`.`%_biobased` AS `%_biobased`,`ni`.`%_recycled` AS `%_recycled`,`ni`.`%_reused` AS `%_reused`,`ni`.`%_landfill` AS `%_landfill`,`ni`.`%_burning` AS `%_burning`,`ni`.`%_recycling` AS `%_recycling`,`ni`.`%_reusing` AS `%_reusing`,`ni`.`dp` AS `dp`,`ni`.`mci` AS `mci`,`ni`.`pci` AS `pci`,`ar`.`archicalc_product_name` AS `archicalc_product_name`,`ar`.`material_costs_per_unit` AS `material_costs_per_unit`,`ar`.`labour_costs_per_unit` AS `labour_costs_per_unit`,`ar`.`source` AS `source` from ((((`classification_table` `c` join `classification_junction_table` `j` on((`c`.`nl_sfb_code` = `j`.`nl_sfb_code`))) join `nmd_db` `n` on((`j`.`nmd_id` = `n`.`nmd_id`))) left join `nibe_db` `ni` on((`n`.`nibe_id` = `ni`.`nibe_id`))) left join `archicalc_db` `ar` on((`n`.`archicalc_id` = `ar`.`archicalc_id`))) */;
/*!50001 SET character_set_client      = @saved_cs_client */;
/*!50001 SET character_set_results     = @saved_cs_results */;
/*!50001 SET collation_connection      = @saved_col_connection */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-03-06 11:16:04
