# -*- coding: utf-8 -*-

import os
import re
import MySQLdb
import urllib2
import urlparse
from private_constant import *

class hatenaFotolife:
	regexStr = "^http://f\\.hatena\\.ne\\.jp/(\\w+)/(\\d+)(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	defaultSizeRegex = re.compile("<div id=\"foto-for-html-tag\" style=\"display:none;\">\\s*<img id=\"foto-for-html-tag-\\d+\" src=\"(http://[a-z0-9/_\\-\\.]+)\" style=\"display:none;\" class=\"[a-z]+\" alt=\".*\" title=\".*\" />\\s*</div>", re.IGNORECASE)
	originalSizeRegex = re.compile(u"<a href=\"(http://[a-z0-9/_\\-\\.]+)\"><img src=\"/images/original.gif\" alt=\"オリジナルサイズを表示\" title=\"オリジナルサイズを表示\" />オリジナルサイズを表示</a>", re.IGNORECASE)
	
	last = None
	
	def getUriData(self, username, id):
		if self.last is not None and self.last[0] == username and str(self.last[1]) == str(id):
			return self.last
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
		c.execute("SELECT * FROM hatena_fotolife WHERE username = %s AND id = %s"
			% (db.literal(username), db.literal(id))
		)
		
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			reqUri = "http://f.hatena.ne.jp/%s/%s" % (username, id)
			httpRes = urllib2.urlopen(reqUri)
			
			if httpRes.geturl() != reqUri: #リダイレクトされたと判断
				return None
				
			html = httpRes.read().decode("utf-8")
			
			if html.find("<img src=\"\" alt=\"\" title=\"\" width=\"\" height=\"\" class=\"foto\" style=\"\" />") != -1:
				return None
			
			originalSize = None
			largeSize = None
			thumbnail = None
			
			match = self.defaultSizeRegex.search(html)
			largeSize = match.group(1)
			
			if html.find("<object data=\"/tools/flvplayer.swf\" type=\"application/x-shockwave-flash\"") == -1:
				#画像
				match = self.originalSizeRegex.search(html)
				if match is not None:
					originalSize = match.group(1)
				else:
					originalSize = largeSize
			else:
				#動画
				originalSize = "http://cdn-ak.f.st-hatena.com/images/fotolife/%s/%s/%s/%s.flv" % (username[0], username, id[0:8], id)
			
			thumbnail = "http://cdn-ak.f.st-hatena.com/images/fotolife/%s/%s/%s/%s_120.jpg" % (username[0], username, id[0:8], id)
			
			c.execute("INSERT INTO hatena_fotolife VALUES (%s, %s, %s, %s, %s)"
				% (db.literal(username), db.literal(id), db.literal(originalSize), db.literal(largeSize), db.literal(thumbnail))
			)
			
			db.commit()
			
			self.last = [username, id, originalSize, largeSize, thumbnail]
		else:
			self.last = sqlResult
		
		return self.last
	
	def getFullSize(self, match):
		data = self.getUriData(match.group(1), match.group(2))
		
		if data is None:
			return None
		
		ret = data[2]
		
		query = dict(urlparse.parse_qsl(os.environ.get("QUERY_STRING", "")))
		if "movie" in query and query["movie"] == "true":
			return ret
		else:
			return ret.replace(".flv", ".jpg")
	
	def getLargeSize(self, match):
		data = self.getUriData(match.group(1), match.group(2))
		
		if data is None:
			return None
		else:
			return data[3]
	
	def getThumbnail(self, match):
		data = self.getUriData(match.group(1), match.group(2))
		
		if data is None:
			return None
		else:
			return data[4]
