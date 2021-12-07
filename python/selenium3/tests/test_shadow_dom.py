import os

import pytest

from selenium.webdriver import Chrome
from selenium.webdriver import Firefox
from selenium.webdriver import Remote
from selenium.webdriver.remote.webelement import WebElement
from selenium.webdriver.chrome.options import Options as ChromeOptions


def test_old_way_chrome95():
    options = ChromeOptions()
    options.set_capability('browserVersion', '95.0')

    sauce_options = {'username': os.environ["SAUCE_USERNAME"],
                     'accessKey': os.environ["SAUCE_ACCESS_KEY"]}
    options.set_capability('sauce:options', sauce_options)

    sauce_url = "https://ondemand.us-west-1.saucelabs.com/wd/hub"

    driver = Remote(command_executor=sauce_url, options=options)

    driver.get('http://watir.com/examples/shadow_dom.html')

    shadow_host = driver.find_element_by_css_selector('#shadow_host')
    shadow_root = driver.execute_script('return arguments[0].shadowRoot', shadow_host)

    shadow_content = shadow_root.find_element_by_css_selector('#shadow_content')

    assert shadow_content.text == 'some text'

    driver.quit()


def test_old_way_chrome96():
    driver = Chrome()

    driver.get('http://watir.com/examples/shadow_dom.html')

    shadow_host = driver.find_element_by_css_selector('#shadow_host')
    shadow_root = driver.execute_script('return arguments[0].shadowRoot', shadow_host)

    with pytest.raises(AttributeError):
        shadow_root.find_element_by_css_selector('#shadow_content')


def test_hack_chrome96():
    driver = Chrome()

    driver.get('http://watir.com/examples/shadow_dom.html')

    shadow_host = driver.find_element_by_css_selector('#shadow_host')
    shadow_root_dict = driver.execute_script('return arguments[0].shadowRoot', shadow_host)
    shadow_root_id = shadow_root_dict['shadow-6066-11e4-a52e-4f735466cecf']

    shadow_root = WebElement(driver, shadow_root_id, w3c=True)
    shadow_content = shadow_root.find_element_by_css_selector('#shadow_content')

    assert shadow_content.text == 'some text'

    driver.quit()


def test_weird_firefox():
    driver = Firefox()

    driver.get('http://watir.com/examples/shadow_dom.html')

    shadow_host = driver.find_element_by_css_selector('#shadow_host')
    children = driver.execute_script('return arguments[0].shadowRoot.children', shadow_host)

    shadow_content = next(child for child in children if child.get_attribute('id') == 'shadow_content')

    assert shadow_content.text == 'some text'

    driver.quit()
