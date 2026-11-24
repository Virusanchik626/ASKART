-- MySQL dump 10.13  Distrib 8.0.44, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: askart
-- ------------------------------------------------------
-- Server version	8.0.46

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
-- Table structure for table `chatmembers`
--

DROP TABLE IF EXISTS `chatmembers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `chatmembers` (
  `chat_id` int NOT NULL,
  `user_id` int NOT NULL,
  `role` enum('Создатель','Администратор','Участник') NOT NULL,
  PRIMARY KEY (`chat_id`,`user_id`),
  KEY `user_idx` (`user_id`),
  KEY `chat_idx` (`chat_id`),
  CONSTRAINT `chat` FOREIGN KEY (`chat_id`) REFERENCES `chats` (`id_chat`),
  CONSTRAINT `user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id_user`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `chatmembers`
--

LOCK TABLES `chatmembers` WRITE;
/*!40000 ALTER TABLE `chatmembers` DISABLE KEYS */;
INSERT INTO `chatmembers` VALUES (1,1,'Участник'),(1,2,'Участник'),(2,1,'Создатель'),(3,2,'Участник'),(4,2,'Участник'),(4,3,'Участник'),(5,1,'Участник'),(5,3,'Участник'),(6,2,'Участник'),(6,5,'Участник'),(7,5,'Создатель'),(8,2,'Создатель'),(9,2,'Участник'),(9,6,'Участник'),(10,2,'Участник'),(10,9,'Участник'),(11,9,'Создатель'),(12,9,'Создатель'),(13,1,'Участник'),(13,9,'Участник');
/*!40000 ALTER TABLE `chatmembers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `chats`
--

DROP TABLE IF EXISTS `chats`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `chats` (
  `id_chat` int NOT NULL,
  `name` varchar(50) NOT NULL,
  `type_chat` enum('Личный','Группа') NOT NULL,
  `type_group` enum('Личная','Рабочая') DEFAULT NULL,
  `user_id` int NOT NULL,
  PRIMARY KEY (`id_chat`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `chats`
--

LOCK TABLES `chats` WRITE;
/*!40000 ALTER TABLE `chats` DISABLE KEYS */;
INSERT INTO `chats` VALUES (1,'Сальманов Радмир','Личный','Личная',1),(2,'Крутая группа 23 П-2','Группа','Личная',1),(3,'Сальманов Радмир','Личный',NULL,3),(4,'Сальманов Радмир','Личный',NULL,3),(5,'Иванов Иван','Личный',NULL,1),(6,'Сальманов Радмир','Личный',NULL,5),(7,'тестовая-группа','Группа','Личная',5),(8,'тестовая-канбан','Группа','Рабочая',2),(9,'Сальманов Радмир','Личный',NULL,6),(10,'Сальманов Радмир','Личный',NULL,9),(11,'ТЕст','Группа','Личная',9),(12,'Рабочая группа','Группа','Рабочая',9),(13,'Псянчин Аскар','Личный',NULL,9);
/*!40000 ALTER TABLE `chats` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `kanban_columns`
--

DROP TABLE IF EXISTS `kanban_columns`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `kanban_columns` (
  `id_column` int NOT NULL,
  `chat_id` int NOT NULL,
  `name` varchar(50) NOT NULL,
  `color` varchar(20) NOT NULL,
  `position` int NOT NULL,
  PRIMARY KEY (`id_column`,`chat_id`),
  KEY `chat_id` (`chat_id`),
  CONSTRAINT `kanban_columns_ibfk_1` FOREIGN KEY (`chat_id`) REFERENCES `chats` (`id_chat`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `kanban_columns`
--

LOCK TABLES `kanban_columns` WRITE;
/*!40000 ALTER TABLE `kanban_columns` DISABLE KEYS */;
INSERT INTO `kanban_columns` VALUES (1,8,'Новая','#9E9E9E',1),(1,12,'Новая','#9E9E9E',1),(2,8,'В работе','#5DD4FF',3),(2,12,'В работе','#5DD4FF',2),(3,8,'Выполнена','#4CAF50',2),(3,12,'Выполнена','#4CAF50',3);
/*!40000 ALTER TABLE `kanban_columns` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `messages`
--

DROP TABLE IF EXISTS `messages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `messages` (
  `id_message` int NOT NULL,
  `text` text NOT NULL,
  `chat_id` int NOT NULL,
  `sender_id` int NOT NULL,
  `reply_id` int DEFAULT NULL,
  `send_time` datetime NOT NULL,
  `edit_time` datetime DEFAULT NULL,
  PRIMARY KEY (`id_message`),
  KEY `chat1_idx` (`chat_id`),
  KEY `user1_idx` (`sender_id`),
  CONSTRAINT `chat1` FOREIGN KEY (`chat_id`) REFERENCES `chats` (`id_chat`),
  CONSTRAINT `user1` FOREIGN KEY (`sender_id`) REFERENCES `users` (`id_user`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `messages`
--

LOCK TABLES `messages` WRITE;
/*!40000 ALTER TABLE `messages` DISABLE KEYS */;
INSERT INTO `messages` VALUES (1,'Изменённый текст сообщения',1,1,NULL,'2026-09-14 13:29:04','2026-10-03 08:56:03'),(2,'сальманов не соси хуй',2,1,NULL,'2026-09-14 13:30:03','2026-09-14 13:37:05'),(3,'сальманов соси хуй',2,1,2,'2026-09-14 13:30:58',NULL),(4,'Салам алейкум, брат. 2717 рублей на карте есть??',5,1,NULL,'2026-10-26 10:11:47',NULL),(5,'ы ыыыы',6,5,NULL,'2026-10-26 10:42:09',NULL),(6,'Хорошо, спасибо',6,2,NULL,'2026-10-26 10:43:40','2026-10-26 10:43:48'),(7,'Окей, Бро',6,5,6,'2026-10-26 11:13:21',NULL),(8,'Привет, Сальманов. Ты где??',9,6,NULL,'2026-10-26 11:17:06',NULL),(10,'7446',9,2,NULL,'2026-10-26 11:18:55','2026-10-26 11:21:13'),(11,'орпопроппр',9,2,NULL,'2026-10-26 11:18:58',NULL),(12,'цуйцуйцуйуцй13213цуйцуйцуйуцй13213цуйцуйцуйуцй13213цуйцуйцуйуцй13213цуйцуйцуйуцй13213цуйцуйцуйуцй13213цуйцуйцуйуцй13213цуйцуйцуйуцй13213цуйцуйцуйуцй13213цуйцуйцуйуцй13213цуйцуйцуйуцй13213цуйцуйцуйуцй1',9,2,10,'2026-10-26 11:20:38',NULL),(13,'Привет, Сальманов!',10,9,NULL,'2026-11-11 07:46:53',NULL),(14,'вцдвзйцдвзцйдвхйцвд\r\nвцдвзйцдвзцйдвхйцвд\r\nвцдвзйцдвзцйдвхйцвд\r\nвцдвзйцдвзцйдвхйцвд\r\nвцдвзйцдвзцйдвхйцвд\r\nвцдвзйцдвзцйдвхйцвд\r\nвцдвзйцдвзцйдвхйцвд\r\nвцдвзйцдвзцйдвхйцвд\r\nвцдвзйцдвзцйдвхйцвд\r\nвцдвзйцдв❤️',10,9,NULL,'2026-11-11 08:16:40',NULL),(15,'дадада',12,9,NULL,'2026-11-16 17:28:44',NULL),(16,'Привет, Аскар!',13,9,NULL,'2026-11-16 17:59:50',NULL);
/*!40000 ALTER TABLE `messages` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `task_assignees`
--

DROP TABLE IF EXISTS `task_assignees`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `task_assignees` (
  `task_id` int NOT NULL,
  `user_id` int NOT NULL,
  KEY `user5_idx` (`user_id`),
  KEY `task_idx` (`task_id`),
  CONSTRAINT `task` FOREIGN KEY (`task_id`) REFERENCES `tasks` (`id_task`),
  CONSTRAINT `user5` FOREIGN KEY (`user_id`) REFERENCES `users` (`id_user`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `task_assignees`
--

LOCK TABLES `task_assignees` WRITE;
/*!40000 ALTER TABLE `task_assignees` DISABLE KEYS */;
/*!40000 ALTER TABLE `task_assignees` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tasks`
--

DROP TABLE IF EXISTS `tasks`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tasks` (
  `id_task` int NOT NULL,
  `chat_id` int NOT NULL,
  `name` varchar(50) NOT NULL,
  `describe` text,
  `status` enum('Новая','В работе','Выполнена') NOT NULL,
  `deadline` datetime NOT NULL,
  `priority` enum('Низкий','Средний','Высокий') NOT NULL,
  `creator_id` int NOT NULL,
  `assigne_user_id` int NOT NULL,
  PRIMARY KEY (`id_task`),
  KEY `chat_idx` (`chat_id`),
  KEY `chat2_idx` (`chat_id`),
  KEY `user_idx` (`creator_id`),
  CONSTRAINT `chat2` FOREIGN KEY (`chat_id`) REFERENCES `chats` (`id_chat`),
  CONSTRAINT `user2` FOREIGN KEY (`creator_id`) REFERENCES `users` (`id_user`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tasks`
--

LOCK TABLES `tasks` WRITE;
/*!40000 ALTER TABLE `tasks` DISABLE KEYS */;
INSERT INTO `tasks` VALUES (1,8,'Сдать работу','Блабла','Выполнена','2026-11-02 00:00:00','Средний',2,2);
/*!40000 ALTER TABLE `tasks` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `id_user` int NOT NULL,
  `surname` varchar(50) NOT NULL,
  `name` varchar(50) NOT NULL,
  `lastname` varchar(50) DEFAULT NULL,
  `login` varchar(30) NOT NULL,
  `password` varchar(20) NOT NULL,
  `avatar` mediumblob,
  `number` varchar(12) NOT NULL,
  `data_reg` datetime NOT NULL,
  `email` varchar(50) DEFAULT NULL,
  `is_admin` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id_user`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,'Псянчин','Аскар','Азатович','xxless','xxless',NULL,'89870517037','2026-09-08 12:37:33','xxless52@gmail.com',0),(2,'Сальманов','Радмир','Кемович','salma','salma1',NULL,'89054893544','2026-09-08 13:33:45','salma@gmail.com',0),(3,'Иванов','Иван','Иванович','testuser111','Test12345',NULL,'+77529232895','2026-10-03 08:25:05','testuser001@example.com',0),(4,'Иванов','Иван','Иванович','testuser002','Test12345',NULL,'+77529232896','2026-10-03 08:56:02','testuser002@example.com',0),(5,'тестов','тестовый юзер','','test@test.test','qwerty123',NULL,'+79991234567','2026-10-26 10:40:52','test@test.test',0),(6,'Воробьев','Артём','','test@gmail.com','qwerty123',NULL,'+79991327654','2026-10-26 11:16:17','test@gmail.com',0),(7,'Служителев','Артём','','er@ro.mailr','testtest',NULL,'+79999996767','2026-11-09 08:05:05','er@ro.mailr',0),(8,'Служителев','Артём','','er@ro.mail.r','testtest',NULL,'+79999995555','2026-11-09 08:17:47','er@ro.mail.r',0),(9,'Служителев','Артём','','artem@mail.ru','testtest',NULL,'+79999999999','2026-11-11 07:39:01','artem@mail.ru',0),(10,'Админов','Админ','Админович','admin','adminadmin',NULL,'+46717788889','2026-11-17 19:59:21','admin@mail.ru',1);
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-11-18 11:28:57
