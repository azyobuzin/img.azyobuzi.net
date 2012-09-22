# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
import json
from private_constant import *

class dailymotion:
	def __str__(self):
		return "Dailymotion"
	
	regexStr = "^https?://(?:www\\.)?dailymotion\\.com/video/([\\w\\-]+)/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	last = None
	
	def getUriData(self, match):
		id = match.group(1)
		
		if self.last is not None and self.last[0] == id:
			return self.last
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
		c.execute("SELECT * FROM dailymotion WHERE id = " + db.literal(id))
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			httpRes = None
			
			try:
				httpRes = urllib2.urlopen("https://api.dailymotion.com/video/%s?fields=thumbnail_large_url,thumbnail_medium_url,thumbnail_url" % urllib2.quote(id))
			except:
				return None
			
			j = json.loads(httpRes.read())
			
			thumbnail_large = j["thumbnail_large_url"]
			thumbnail_medium = j["thumbnail_medium_url"]
			thumbnail = j["thumbnail_url"]
			
			c.execute("INSERT INTO dailymotion VALUES (%s, %s, %s, %s)"
				% (db.literal(id), db.literal(thumbnail_large), db.literal(thumbnail_medium), db.literal(thumbnail))
			)
			
			db.commit()
			
			self.last = [id, thumbnail_large, thumbnail_medium, thumbnail]
		else:
			self.last = sqlResult
		
		return self.last
	
	def getFullSize(self, match):
		data = self.getUriData(match)
		return data[3] if data is not None else None
	
	def getLargeSize(self, match):
		data = self.getUriData(match)
		return data[1] if data is not None else None
	
	def getThumbnail(self, match):
		data = self.getUriData(match)
		return data[2] if data is not None else None
