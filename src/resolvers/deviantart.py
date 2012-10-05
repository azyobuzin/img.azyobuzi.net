# -*- coding: utf-8 -*-

import re
import json
import urllib2
import MySQLdb
from private_constant import *

class deviantart:
	def __str__(self):
		return "deviantART"
	
	regexStr = "^https?://([\\w\\-]+)\\.deviantart\\.com/art/([\\w\\-]+)/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	last = None
	
	def getUriData(self, match):
		username = match.group(1)
		id = match.group(2)
		
		if self.last is not None and self.last[0] == username and self.last[1] == id:
			return self.last
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
		c.execute("SELECT * FROM deviantart WHERE username = %s AND id = %s" % (db.literal(username), db.literal(id)))
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			httpRes = None
			
			try:
				httpRes = urllib2.urlopen(
					"http://backend.deviantart.com/oembed?url="
					+ urllib2.quote("http://%s.deviantart.com/art/%s" % (username, id))
				)
			except:
				return None
			
			j = json.loads(httpRes.read())
			
			full = j["url"]
			thumbnail = j["thumbnail_url"]
			thumbnail150 = j["thumbnail_url_150"]
			
			c.execute("INSERT INTO deviantart VALUES (%s, %s, %s, %s, %s)"
				% (db.literal(username), db.literal(id), db.literal(full), db.literal(thumbnail), db.literal(thumbnail150))
			)
			
			db.commit()
			
			self.last = [username, id, full, thumbnail, thumbnail150]
		else:
			self.last = sqlResult
		
		return self.last
	
	def getFullSize(self, match):
		data = self.getUriData(match)
		return data[2] if data is not None else None
	
	def getLargeSize(self, match):
		data = self.getUriData(match)
		return data[3] if data is not None else None
	
	def getThumbnail(self, match):
		data = self.getUriData(match)
		return data[4] if data is not None else None
