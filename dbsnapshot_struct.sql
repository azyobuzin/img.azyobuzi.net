-- phpMyAdmin SQL Dump
-- version 3.5.2
-- http://www.phpmyadmin.net
--
-- ホスト: localhost
-- 生成日時: 2012 年 11 月 22 日 22:15
-- サーバのバージョン: 5.1.22-rc
-- PHP のバージョン: 5.2.5

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- データベース: `azyobuzin_img`
--

-- --------------------------------------------------------

--
-- テーブルの構造 `cloudapp`
--

CREATE TABLE IF NOT EXISTS `cloudapp` (
  `id` varchar(20) NOT NULL,
  `remote` tinytext NOT NULL,
  `thumbnail` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `dailymotion`
--

CREATE TABLE IF NOT EXISTS `dailymotion` (
  `id` varchar(255) NOT NULL,
  `thumbnail_large` tinytext NOT NULL,
  `thumbnail_medium` tinytext NOT NULL,
  `thumbnail` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `deviantart`
--

CREATE TABLE IF NOT EXISTS `deviantart` (
  `username` varchar(20) NOT NULL,
  `id` varchar(255) NOT NULL,
  `full` tinytext NOT NULL,
  `thumbnail` tinytext NOT NULL,
  `thumbnail150` tinytext NOT NULL,
  PRIMARY KEY (`username`,`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `flickr`
--

CREATE TABLE IF NOT EXISTS `flickr` (
  `id` bigint(20) unsigned NOT NULL,
  `thumbnail` tinytext NOT NULL,
  `medium` tinytext NOT NULL,
  `original` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `fxcamera`
--

CREATE TABLE IF NOT EXISTS `fxcamera` (
  `id` varchar(10) NOT NULL,
  `scaled` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `gochisophoto`
--

CREATE TABLE IF NOT EXISTS `gochisophoto` (
  `id` varchar(40) NOT NULL,
  `prefix` tinytext NOT NULL,
  `postfix` varchar(20) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `hatena_fotolife`
--

CREATE TABLE IF NOT EXISTS `hatena_fotolife` (
  `username` varchar(32) NOT NULL,
  `id` bigint(20) unsigned NOT NULL,
  `full` tinytext NOT NULL,
  `large` tinytext NOT NULL,
  `thumb` tinytext NOT NULL,
  PRIMARY KEY (`username`,`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `movapic`
--

CREATE TABLE IF NOT EXISTS `movapic` (
  `id` int(10) unsigned NOT NULL,
  `expanded` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `niconico`
--

CREATE TABLE IF NOT EXISTS `niconico` (
  `id` varchar(15) NOT NULL,
  `thumbnail` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `owly`
--

CREATE TABLE IF NOT EXISTS `owly` (
  `id` varchar(10) NOT NULL,
  `original` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `path`
--

CREATE TABLE IF NOT EXISTS `path` (
  `id` varchar(10) NOT NULL,
  `prefix` tinytext NOT NULL,
  `extension` varchar(5) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `photohito`
--

CREATE TABLE IF NOT EXISTS `photohito` (
  `id` int(10) unsigned NOT NULL,
  `prefix` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `photomemo`
--

CREATE TABLE IF NOT EXISTS `photomemo` (
  `id` int(10) unsigned NOT NULL,
  `filename` varchar(50) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `photozou`
--

CREATE TABLE IF NOT EXISTS `photozou` (
  `id` int(10) unsigned NOT NULL,
  `image` tinytext NOT NULL,
  `original_image` tinytext NOT NULL,
  `thumbnail_image` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `piapro`
--

CREATE TABLE IF NOT EXISTS `piapro` (
  `id` varchar(10) NOT NULL,
  `prefix` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `pixiv`
--

CREATE TABLE IF NOT EXISTS `pixiv` (
  `id` int(11) NOT NULL,
  `prefix` tinytext NOT NULL,
  `extension` varchar(5) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `tinami`
--

CREATE TABLE IF NOT EXISTS `tinami` (
  `cont_id` int(10) unsigned NOT NULL,
  `thumbnail` tinytext NOT NULL,
  `image` tinytext NOT NULL,
  PRIMARY KEY (`cont_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `tumblr`
--

CREATE TABLE IF NOT EXISTS `tumblr` (
  `id` bigint(20) unsigned NOT NULL,
  `original` tinytext NOT NULL,
  `large` tinytext NOT NULL,
  `thumb` tinytext NOT NULL,
  `video` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `tumblr_shorten`
--

CREATE TABLE IF NOT EXISTS `tumblr_shorten` (
  `shorten` varchar(15) NOT NULL,
  `hostname` tinytext NOT NULL,
  `id` bigint(20) unsigned NOT NULL,
  PRIMARY KEY (`shorten`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `tunabe`
--

CREATE TABLE IF NOT EXISTS `tunabe` (
  `id` varchar(10) NOT NULL,
  `org` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `twitcasting`
--

CREATE TABLE IF NOT EXISTS `twitcasting` (
  `id` int(10) unsigned NOT NULL,
  `thumbnail` tinytext NOT NULL,
  `thumbnailsmall` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `twitter`
--

CREATE TABLE IF NOT EXISTS `twitter` (
  `id` bigint(20) unsigned NOT NULL,
  `media` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `ustream`
--

CREATE TABLE IF NOT EXISTS `ustream` (
  `channel` varchar(255) NOT NULL,
  `id` bigint(20) NOT NULL,
  `small` tinytext NOT NULL,
  `medium` tinytext NOT NULL,
  PRIMARY KEY (`channel`,`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `vimeo`
--

CREATE TABLE IF NOT EXISTS `vimeo` (
  `id` int(10) unsigned NOT NULL,
  `small` tinytext NOT NULL,
  `large` tinytext NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- テーブルの構造 `yfrog`
--

CREATE TABLE IF NOT EXISTS `yfrog` (
  `hash` varchar(15) NOT NULL,
  `image` tinytext NOT NULL,
  PRIMARY KEY (`hash`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;