# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
from private_constant import *

class fxcamera:
	def __str__(self):
		return "FxCamera"
	
	regexStr = "^https?://fxc\\.am/p/(\\w+)/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	photoRegex = re.compile("<img id='photo' src='(http://[a-za-z0-9/_\\-\\.]+)'>")
	
	last = None
	
	def getUriData(self, match):
		id = match.group(1)
		
		if self.last is not None and str(self.last[0]) == id:
			return self.last
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
		c.execute("SELECT * FROM fxcamera WHERE id = " + db.literal(id))
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			httpRes = None
			
			try:
				httpRes = urllib2.urlopen("http://fxc.am/p/" + id)
			except:
				return None
			
			html = httpRes.read().decode("utf-8")
			match = self.photoRegex.search(html)
			scaled = match.group(1)
			
			c.execute("INSERT INTO fxcamera VALUES (%s, %s)"
				% (db.literal(id), db.literal(scaled))
			)
			
			db.commit()
			
			self.last = [id, scaled]
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
		return data[1] if data is not None else None
