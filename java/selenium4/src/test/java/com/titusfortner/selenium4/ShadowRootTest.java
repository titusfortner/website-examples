package com.titusfortner.selenium4;

import io.github.bonigarcia.wdm.WebDriverManager;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.openqa.selenium.By;
import org.openqa.selenium.JavascriptExecutor;
import org.openqa.selenium.SearchContext;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.chrome.ChromeDriver;
import org.openqa.selenium.edge.EdgeDriver;
import org.openqa.selenium.firefox.FirefoxDriver;

import java.util.List;
public class ShadowRootTest {

    WebDriver driver;
    private WebElement shadowRoot;

    // What most people use for Shadow DOM Elements;
    // Still works for Chrome < v96, Edge < v96, Safari
    @Test
    public void getShadowRootOldWayOldChrome() {
        WebDriverManager.edgedriver().setup();
        driver = new EdgeDriver();
        driver.get("http://watir.com/examples/shadow_dom.html");

        WebElement shadowHost = driver.findElement(By.cssSelector("#shadow_host"));
        JavascriptExecutor jsDriver = (JavascriptExecutor) driver;

        WebElement shadowRoot = (WebElement) jsDriver.executeScript("return arguments[0].shadowRoot", shadowHost);
        WebElement shadowContent = shadowRoot.findElement(By.cssSelector("#shadow_content"));

        Assertions.assertEquals("some text", shadowContent.getText());
    }

    // Same method as above, with Chrome v96 throws Cast Exception
    @Test
    public void getShadowRootOldWayNewChrome() {
        WebDriverManager.chromedriver().setup();
        driver = new ChromeDriver();
        driver.get("http://watir.com/examples/shadow_dom.html");

        WebElement shadowHost = driver.findElement(By.cssSelector("#shadow_host"));
        JavascriptExecutor jsDriver = (JavascriptExecutor) driver;

        Object shadowRootObject = jsDriver.executeScript("return arguments[0].shadowRoot", shadowHost);
        Assertions.assertThrows(ClassCastException.class, () -> shadowRoot = (WebElement) shadowRootObject);
    }

    // Fix Cast Exception with `SearchContext`
    @Test
    public void getShadowRootNewChromeFix() {
        WebDriverManager.chromedriver().setup();
        driver = new ChromeDriver();
        driver.get("http://watir.com/examples/shadow_dom.html");

        WebElement shadowHost = driver.findElement(By.cssSelector("#shadow_host"));
        JavascriptExecutor jsDriver = (JavascriptExecutor) driver;

        SearchContext shadowRoot = (SearchContext) jsDriver.executeScript("return arguments[0].shadowRoot", shadowHost);
        WebElement shadowContent = shadowRoot.findElement(By.cssSelector("#shadow_content"));

        Assertions.assertEquals("some text", shadowContent.getText());
    }

    // Firefox is special
    @Test
    public void getShadowRootFirefox() {
        WebDriverManager.firefoxdriver().setup();
        driver = new FirefoxDriver();
        driver.get("http://watir.com/examples/shadow_dom.html");

        WebElement shadowHost = driver.findElement(By.cssSelector("#shadow_host"));
        JavascriptExecutor jsDriver = (JavascriptExecutor) driver;

        List<WebElement> children = (List<WebElement>) jsDriver.executeScript("return arguments[0].shadowRoot.children", shadowHost);

        WebElement shadowContent = null;
        for (WebElement element : children) {
            if (element.getAttribute("id").equals("shadow_content")) {
                shadowContent = element;
                break;
            }
        }

        assert shadowContent != null;
        Assertions.assertEquals("some text", shadowContent.getText());
    }

    // Please use this one!
    @Test
    public void getShadowRootNewChromeRecommended() {
        WebDriverManager.chromedriver().setup();
        driver = new ChromeDriver();

        driver.get("http://watir.com/examples/shadow_dom.html");

        WebElement shadowHost = driver.findElement(By.cssSelector("#shadow_host"));
        WebElement shadowContent = shadowHost.getShadowRoot().findElement(By.cssSelector("#shadow_content"));

        Assertions.assertEquals("some text", shadowContent.getText());
    }

    @AfterEach
    public void quit() {
        if (driver != null) {
            driver.quit();
        }
    }
}
