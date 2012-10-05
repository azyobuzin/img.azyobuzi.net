# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
from private_constant import *

class piapro:
	def __str__(self):
		return "PIAPRO"
	
	regexStr = "^https?://(?:www\\.)?piapro\\.jp/t/(\\w+)(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	imageRegex = re.compile("<meta property=\"og:image\" content=\"(http://[a-z0-9/_\\-\\.]+)_0500_0500.jpg\" />", re.IGNORECASE)
	
	last = None
	
	def getUriData(self, match):
		id = match.group(1)
		
		if self.last is not None and str(self.last[0]) == id:
			return self.last
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
		c.execute("SELECT * FROM piapro WHERE id = " + db.literal(id))
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			httpRes = None
			
			try:
				httpRes = urllib2.urlopen("http://piapro.jp/t/" + id)
			except:
				return None
			
			html = httpRes.read().decode("Shift_JIS")
			
			match = self.imageRegex.search(html)
			
			if match is None:
				return None
			
			prefix = match.group(1)
			
			c.execute("INSERT INTO piapro VALUES (%s, %s)"
				% (db.literal(id), db.literal(prefix))
			)
			
			db.commit()
			
			self.last = [id, prefix]
		else:
			self.last = sqlResult
		
		return self.last
	
	def getFullSize(self, match):
		data = self.getUriData(match)
		
		return (data[1] + "_0500_0500.jpg") if data is not None else None
	
	def getLargeSize(self, match):
		data = self.getUriData(match)
		
		return (data[1] + "_0500_0500.jpg") if data is not None else None
	
	def getThumbnail(self, match):
		data = self.getUriData(match)
		
		return (data[1] + "_0120_0120.jpg") if data is not None else None
