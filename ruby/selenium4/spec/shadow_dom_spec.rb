# frozen_string_literal: true
require 'spec_helper'

RSpec.describe 'Shadow DOM' do
  after { @driver.quit }

  context 'when old Chrome' do
    #  What most people use for Shadow DOM Elements;
    #  Still works for Chrome < v96, Edge < v96, Safari
    it "old code works" do
      user = ENV['SAUCE_USERNAME']
      key = ENV['SAUCE_ACCESS_KEY']

      opts = Selenium::WebDriver::Options.chrome(browser_version: '95.0',
                                                 'sauce:options': {username: user,
                                                                   access_key: key})

      @driver = Selenium::WebDriver.for :remote,
                                        url: 'https://ondemand.saucelabs.com/wd/hub/',
                                        capabilities: opts
      @driver.get('http://watir.com/examples/shadow_dom.html')

      shadow_host = @driver.find_element(css: '#shadow_host')
      shadow_root = @driver.execute_script('return arguments[0].shadowRoot', shadow_host)
      shadow_content = shadow_root.find_element(css: '#shadow_content')

      expect(shadow_content.text).to eq 'some text'
    end
  end

  context 'when new Chrome' do
    # Same code as above, but with Chromium 96+, it works in Selenium 4.1 (but not 4.0)
    it 'old code works' do
      @driver = Selenium::WebDriver.for :chrome

      @driver.get('http://watir.com/examples/shadow_dom.html')

      shadow_host = @driver.find_element(css: '#shadow_host')
      shadow_root = @driver.execute_script('return arguments[0].shadowRoot', shadow_host)
      shadow_content = shadow_root.find_element(css: '#shadow_content')

      expect(shadow_content.text).to eq 'some text'
    end

    # Please use this code
    it 'recommended code' do
      @driver = Selenium::WebDriver.for :chrome

      @driver.get('http://watir.com/examples/shadow_dom.html')

      shadow_host = @driver.find_element(css: '#shadow_host')
      shadow_root = shadow_host.shadow_root
      shadow_content = shadow_root.find_element(css: '#shadow_content')

      expect(shadow_content.text).to eq 'some text'
    end
  end

  context 'when Firefox' do
    # Firefox is special
    it 'workaround' do
      @driver = Selenium::WebDriver.for :firefox

      @driver.get('http://watir.com/examples/shadow_dom.html')

      shadow_host = @driver.find_element(css: '#shadow_host')
      children = @driver.execute_script('return arguments[0].shadowRoot.children', shadow_host)

      shadow_content = children.first { |child| child.attribute('id') == 'shadow_content' }

      expect(shadow_content.text).to eq 'some text'
    end
  end
end
