SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;


CREATE TABLE IF NOT EXISTS `cloudapp` (
  `id` varchar(20) NOT NULL,
  `remote` tinytext NOT NULL,
  `thumbnail` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `dailymotion` (
  `id` varchar(255) NOT NULL,
  `thumbnail_large` tinytext NOT NULL,
  `thumbnail_medium` tinytext NOT NULL,
  `thumbnail` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `deviantart` (
  `username` varchar(20) NOT NULL,
  `id` varchar(255) NOT NULL,
  `full` tinytext NOT NULL,
  `thumbnail` tinytext NOT NULL,
  `thumbnail150` tinytext NOT NULL,
  PRIMARY KEY (`username`,`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `flickr` (
  `id` bigint(20) unsigned NOT NULL,
  `thumbnail` tinytext NOT NULL,
  `medium` tinytext NOT NULL,
  `original` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `fxcamera` (
  `id` varchar(10) NOT NULL,
  `scaled` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `gochisophoto` (
  `id` varchar(40) NOT NULL,
  `prefix` tinytext NOT NULL,
  `postfix` varchar(20) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `hatena_fotolife` (
  `username` varchar(32) NOT NULL,
  `id` bigint(20) unsigned NOT NULL,
  `full` tinytext NOT NULL,
  `large` tinytext NOT NULL,
  `thumb` tinytext NOT NULL,
  PRIMARY KEY (`username`,`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `movapic` (
  `id` int(10) unsigned NOT NULL,
  `expanded` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `niconico` (
  `id` varchar(15) NOT NULL,
  `thumbnail` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `owly` (
  `id` varchar(10) NOT NULL,
  `original` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `path` (
  `id` varchar(10) NOT NULL,
  `prefix` tinytext NOT NULL,
  `extension` varchar(5) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `photozou` (
  `id` int(10) unsigned NOT NULL,
  `image` tinytext NOT NULL,
  `original_image` tinytext NOT NULL,
  `thumbnail_image` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `piapro` (
  `id` varchar(10) NOT NULL,
  `prefix` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `pixiv` (
  `id` int(11) NOT NULL,
  `prefix` tinytext NOT NULL,
  `extension` varchar(5) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `tinami` (
  `cont_id` int(10) unsigned NOT NULL,
  `thumbnail` tinytext NOT NULL,
  `image` tinytext NOT NULL,
  PRIMARY KEY (`cont_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `tumblr` (
  `id` bigint(20) unsigned NOT NULL,
  `original` tinytext NOT NULL,
  `large` tinytext NOT NULL,
  `thumb` tinytext NOT NULL,
  `video` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `tumblr_shorten` (
  `shorten` varchar(15) NOT NULL,
  `hostname` tinytext NOT NULL,
  `id` bigint(20) unsigned NOT NULL,
  PRIMARY KEY (`shorten`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `tunabe` (
  `id` varchar(10) NOT NULL,
  `org` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `twitcasting` (
  `id` int(10) unsigned NOT NULL,
  `thumbnail` tinytext NOT NULL,
  `thumbnailsmall` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `twitter` (
  `id` bigint(20) unsigned NOT NULL,
  `media` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `ustream` (
  `channel` varchar(255) NOT NULL,
  `id` bigint(20) NOT NULL,
  `small` tinytext NOT NULL,
  `medium` tinytext NOT NULL,
  PRIMARY KEY (`channel`,`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `vimeo` (
  `id` int(10) unsigned NOT NULL,
  `small` tinytext NOT NULL,
  `large` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
