# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
import json
from private_constant import *

class twitcasting:
	def __str__(self):
		return "TwitCasting"
	
	regexStr = "^https?://(?:www\\.)?twitcasting\\.tv/(?:(\\w+)/?|\\w+/movie/(\\d+))(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	last = None
	
	def getUriData(self, match):
		username = match.group(1)
		id = match.group(2)
		
		if username is not None:
			return [username, "http://twitcasting.tv/%s/thumbstream/liveshot" % username, "http://twitcasting.tv/%s/thumbstream/liveshot-1" % username]
		
		if self.last is not None and str(self.last[0]) == id:
			return self.last
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
		c.execute("SELECT * FROM twitcasting WHERE id = " + db.literal(id))
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			httpRes = urllib2.urlopen("http://api.twitcasting.tv/api/moviestatus?type=json&movieid=" + id)
			content = httpRes.read()
			
			if content == "[]":
				return None
			
			j = json.loads(content)
			
			thumbnail = j["thumbnail"]
			thumbnailsmall = j["thumbnailsmall"]
			
			c.execute("INSERT INTO twitcasting VALUES (%s, %s, %s)"
				% (db.literal(id), db.literal(thumbnail), db.literal(thumbnailsmall))
			)
			
			db.commit()
			
			self.last = [id, thumbnail, thumbnailsmall]
		else:
			self.last = sqlResult
		
		return self.last
	
	def getFullSize(self, match):
		data = self.getUriData(match)
		return data[1] if data is not None else None
	
	def getLargeSize(self, match):
		data = self.getUriData(match)
		return data[1] if data is not None else None
	
	def getThumbnail(self, match):
		data = self.getUriData(match)
		return data[2] if data is not None else None
